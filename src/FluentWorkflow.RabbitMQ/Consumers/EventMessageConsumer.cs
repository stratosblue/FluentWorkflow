using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using FluentWorkflow.MessageDispatch;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 事件消息消费器
/// </summary>
/// <param name="channel">信道</param>
/// <param name="messageConsumeDispatcher">消息消费调度器</param>
/// <param name="objectSerializer">对象序列化器</param>
/// <param name="messageTransmissionTypes">消息传输类型字典</param>
/// <param name="messageHandleOptions">消息处理选项字典</param>
/// <param name="targetEventNames">目标消息名称集合</param>
/// <param name="logger"></param>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public class EventMessageConsumer(IChannel channel,
                                  IMessageConsumeDispatcher messageConsumeDispatcher,
                                  IObjectSerializer objectSerializer,
                                  ImmutableDictionary<string, Type> messageTransmissionTypes,
                                  ImmutableDictionary<string, MessageHandleOptions> messageHandleOptions,
                                  ImmutableHashSet<string> targetEventNames,
                                  ILogger logger)
    : AsyncDefaultBasicConsumer(channel)
{
    #region Public 方法

    /// <inheritdoc/>
    public override async Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var activity = ConsumerActivitySource.StartActivity("ConsumeWorkflowEventMessage", ActivityKind.Consumer);
        var eventName = "UnknownEventName";

        try
        {
            if (properties?.Headers is { } headers
                && TryGetEventName(headers, ref eventName))
            {
                if (!targetEventNames.Contains(eventName))
                {
                    throw new InvalidDataException($"Received non target message {eventName} From [{consumerTag}] by {exchange} -> {routingKey}. If changed the Channel for the message, you may need to manually unbind the routing key of the queue.");
                }

                logger.LogDebug("Start consume message {EventName} From [{ConsumerTag}] by {Exchange} -> {RoutingKey}.", eventName, consumerTag, exchange, routingKey);

                if (activity is not null)
                {
                    activity.DisplayName = $"ConsumeWorkflowEventMessage {eventName}";
                }

                if (messageTransmissionTypes.TryGetValue(eventName, out var transmissionType))
                {
                    var dataTransmissionModel = objectSerializer.Deserialize(body.Span, transmissionType) as IDataTransmissionModel<object>
                                                ?? throw new InvalidDataException($"Deserialize message \"{eventName}\" [{transmissionType}] failed.");
                    var message = dataTransmissionModel.Message;
                    if (activity is not null
                        && dataTransmissionModel.TracingContext is { } tracingContext)
                    {
                        var activityContext = tracingContext.RestoreActivityContext(true);

                        activity.SetParentId(activityContext.TraceId, activityContext.SpanId, activityContext.TraceFlags);
                        activity.AddBaggages(tracingContext.Baggage);
                    }

                    await messageConsumeDispatcher.DispatchAsync(eventName, message, cancellationToken);

                    await Channel.BasicAckAsync(deliveryTag, false, cancellationToken);
                }
                else
                {
                    messageHandleOptions.TryGetValue(eventName, out var handleOptions);
                    await HandleErrorMessageAsync(eventName: eventName,
                                                  consumerTag: consumerTag,
                                                  deliveryTag: deliveryTag,
                                                  redelivered: redelivered,
                                                  exchange: exchange,
                                                  routingKey: routingKey,
                                                  policy: handleOptions?.RequeuePolicy ?? MessageRequeuePolicy.Once,
                                                  delay: handleOptions?.RequeueDelay ?? RabbitMQOptions.MessageRequeueDelay,
                                                  cancellationToken: cancellationToken);
                }
            }
            else
            {
                throw new InvalidDataException("Unprocessable queue message.");
            }
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);

            messageHandleOptions.TryGetValue(eventName, out var handleOptions);

            var requeue = CheckShouldRequeue(handleOptions?.RequeuePolicy ?? MessageRequeuePolicy.Unlimited, redelivered);

            logger.LogError(ex, "Error at rabbitmq HandleBasicDeliver. EventName: {EventName} DeliveryTag {DeliveryTag} [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey}. [Requeue:{Requeue}].", eventName, deliveryTag, consumerTag, exchange, routingKey, requeue);

            if (requeue)
            {
                await Task.Delay(handleOptions?.RequeueDelay ?? RabbitMQOptions.MessageRequeueDelay, cancellationToken);
            }

            await Channel.BasicRejectAsync(deliveryTag, requeue, cancellationToken);
        }
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 检查释放应该重新进入队列
    /// </summary>
    /// <param name="policy"></param>
    /// <param name="redelivered"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual bool CheckShouldRequeue(MessageRequeuePolicy policy, bool redelivered)
    {
        return policy switch
        {
            MessageRequeuePolicy.Never => false,
            MessageRequeuePolicy.Once => redelivered == false,
            _ => true,
        };
    }

    /// <summary>
    /// 尝试获取事件名称
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="eventName"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual bool TryGetEventName(IDictionary<string, object?> headers, ref string eventName)
    {
        if (headers.TryGetValue(RabbitMQOptions.EventNameHeaderKey, out var eventNameValue)
            && eventNameValue is byte[] eventNameBytes)
        {
            eventName = Encoding.UTF8.GetString(eventNameBytes);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 处理接收到的错误分发的消息
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="consumerTag"></param>
    /// <param name="deliveryTag"></param>
    /// <param name="redelivered"></param>
    /// <param name="exchange"></param>
    /// <param name="routingKey"></param>
    /// <param name="policy"></param>
    /// <param name="delay"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task HandleErrorMessageAsync(string eventName,
                                                   string consumerTag,
                                                   ulong deliveryTag,
                                                   bool redelivered,
                                                   string exchange,
                                                   string routingKey,
                                                   MessageRequeuePolicy policy,
                                                   TimeSpan delay,
                                                   CancellationToken cancellationToken)
    {
        var requeue = CheckShouldRequeue(policy, redelivered);

        logger.LogWarning("Consume message error on received {EventName}. Not found consumer for it. If changed the qos for the message, you may need to manually unbind the routing key of the queue. [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey} [Requeue:{Requeue}].", eventName, consumerTag, exchange, routingKey, requeue);

        if (requeue)
        {
            return Task.Delay(delay, cancellationToken)
                       .ContinueWith(async _ =>
                       {
                           try
                           {
                               await Channel.BasicRejectAsync(deliveryTag, true, cancellationToken);
                           }
                           catch (Exception ex)
                           {
                               logger.LogError(ex, "Message {EventName} reject error. [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey}", eventName, consumerTag, exchange, routingKey);
                           }
                       });
        }

        return Channel.BasicRejectAsync(deliveryTag, false, cancellationToken).AsTask();
    }

    #endregion Protected 方法
}
