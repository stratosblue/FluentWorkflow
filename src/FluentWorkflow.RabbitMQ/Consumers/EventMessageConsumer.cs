using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using FluentWorkflow.Diagnostics;
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
/// <param name="diagnosticSource"></param>
/// <param name="logger"></param>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public class EventMessageConsumer(IChannel channel,
                                  IMessageConsumeDispatcher messageConsumeDispatcher,
                                  IObjectSerializer objectSerializer,
                                  ImmutableDictionary<string, Type> messageTransmissionTypes,
                                  ImmutableDictionary<string, MessageHandleOptions> messageHandleOptions,
                                  ImmutableHashSet<string> targetEventNames,
                                  IWorkflowDiagnosticSource diagnosticSource,
                                  ILogger logger)
    : AsyncDefaultBasicConsumer(channel)
{
    #region Public 方法

    /// <inheritdoc/>
    public override async Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        Activity? activity = null;
        var eventName = "UnknownEventName";

        try
        {
            if (properties?.Headers is { } headers
                && TryGetHeaderStringValue(headers, RabbitMQDefinedHeaders.EventName, ref eventName))
            {
                if (!targetEventNames.Contains(eventName))
                {
                    throw new InvalidDataException($"Received non target message {eventName} From [{consumerTag}] by {exchange} -> {routingKey}. If changed the channel for the message, you may need to manually unbind the routing key of the queue.");
                }

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Start consume message {EventName} From [{ConsumerTag}] by {Exchange} -> {RoutingKey}.", eventName, consumerTag, exchange, routingKey);
                }

                if (messageTransmissionTypes.TryGetValue(eventName, out var transmissionType))
                {
                    var dataTransmissionModel = objectSerializer.Deserialize(body.Span, transmissionType) as IDataTransmissionModel<object>
                                                ?? throw new InvalidDataException($"Deserialize message \"{eventName}\" [{transmissionType}] failed.");

                    diagnosticSource.MessageReceived(dataTransmissionModel);

                    Exception? exception = null;
                    try
                    {
                        var message = dataTransmissionModel.Message;
                        if (dataTransmissionModel.TracingContext is { } tracingContext)
                        {
                            var activityContext = tracingContext.RestoreActivityContext(true);

                            activity = ConsumerActivitySource.StartActivity($"ConsumeWorkflowEventMessage {eventName}", ActivityKind.Consumer, activityContext);

                            activity?.AddBaggages(tracingContext.Baggage);
                        }

                        await messageConsumeDispatcher.DispatchAsync(eventName, message, cancellationToken);

                        await Channel.BasicAckAsync(deliveryTag, false, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        throw;
                    }
                    finally
                    {
                        diagnosticSource.MessageHandleFinished(dataTransmissionModel, exception);
                    }
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

            var requeue = false;

            //异常如果关联于消费者繁忙，直接进行重入队列，尝试进行重新负载，不进行其它判断
            if (ex is IBusyConsumer)
            {
                requeue = true;
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(ex, "Error at rabbitmq HandleBasicDeliver because of BusyConsumer. EventName: {EventName} DeliveryTag {DeliveryTag} [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey}. [Requeue:{Requeue}].", eventName, deliveryTag, consumerTag, exchange, routingKey, requeue);
                }
            }

            messageHandleOptions.TryGetValue(eventName, out var handleOptions);

            if (!requeue)
            {
                requeue = CheckShouldRequeue(handleOptions?.RequeuePolicy ?? MessageRequeuePolicy.Unlimited, redelivered);

                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex, "Error at rabbitmq HandleBasicDeliver. EventName: {EventName} DeliveryTag {DeliveryTag} [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey}. [Requeue:{Requeue}].", eventName, deliveryTag, consumerTag, exchange, routingKey, requeue);
                }
            }

            if (requeue)
            {
                await Task.Delay(handleOptions?.RequeueDelay ?? RabbitMQOptions.MessageRequeueDelay, cancellationToken);
            }

            await Channel.BasicRejectAsync(deliveryTag, requeue, cancellationToken);
        }
        finally
        {
            activity?.Dispose();
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

        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("Consume message error on received {EventName}. Not found consumer for it. If changed the qos for the message, you may need to manually unbind the routing key of the queue. [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey} [Requeue:{Requeue}].", eventName, consumerTag, exchange, routingKey, requeue);
        }

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
                               if (logger.IsEnabled(LogLevel.Error))
                               {
                                   logger.LogError(ex, "Message {EventName} reject error. [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey}", eventName, consumerTag, exchange, routingKey);
                               }
                           }
                       });
        }

        return Channel.BasicRejectAsync(deliveryTag, false, cancellationToken).AsTask();
    }

    /// <summary>
    /// 尝试获取Header的字符串值
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual bool TryGetHeaderStringValue(IDictionary<string, object?>? headers, string key, [NotNullWhen(true)][NotNullIfNotNull(nameof(value))] ref string? value)
    {
        if (headers?.TryGetValue(key, out var valueValue) == true
            && valueValue is byte[] valueBytes)
        {
            value = Encoding.UTF8.GetString(valueBytes);
            return true;
        }
        return false;
    }

    #endregion Protected 方法
}
