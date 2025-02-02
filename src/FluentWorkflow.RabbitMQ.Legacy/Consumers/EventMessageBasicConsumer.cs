using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

internal abstract class EventMessageBasicConsumer : AsyncDefaultBasicConsumer
{
    #region Protected 字段

    protected readonly IWorkflowDiagnosticSource DiagnosticSource;

    protected readonly ILogger Logger;

    protected readonly IObjectSerializer ObjectSerializer;

    protected readonly CancellationToken RunningCancellationToken;

    protected readonly IServiceScopeFactory ServiceScopeFactory;

    #endregion Protected 字段

    #region Public 构造函数

    public EventMessageBasicConsumer(IModel model,
                                     IServiceScopeFactory serviceScopeFactory,
                                     IObjectSerializer objectSerializer,
                                     IWorkflowDiagnosticSource diagnosticSource,
                                     ILogger logger,
                                     CancellationToken runningCancellationToken)
        : base(model)
    {
        ServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        ObjectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        DiagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        RunningCancellationToken = runningCancellationToken;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override sealed async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
    {
        try
        {
            await InternalHandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
        }
        catch (Exception ex)
        {
            Model.BasicReject(deliveryTag, true);

            Logger.LogError(ex, "Error at rabbitmq HandleBasicDeliver. DeliveryTag {DeliveryTag} [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey}. requeued.", deliveryTag, consumerTag, exchange, routingKey);

            throw;
        }
    }

    #endregion Public 方法

    #region Protected 方法

    public Task HandleErrorMessageAsync(string? eventName,
                                        string consumerTag,
                                        ulong deliveryTag,
                                        bool redelivered,
                                        string exchange,
                                        string routingKey,
                                        MessageRequeuePolicy policy,
                                        TimeSpan delay,
                                        bool forceNotRequeue)
    {
        if (!forceNotRequeue)
        {
            var requeue = CheckShouldRequeue(policy, redelivered);

            Logger.LogWarning("Consume message error on received {EventName}. Not found consumer for it. If changed the qos for the message, you may need to manually unbind the routing key of the queue. [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey} [Requeue:{Requeue}].", eventName, consumerTag, exchange, routingKey, requeue);

            if (requeue)
            {
                return Task.Delay(delay, RunningCancellationToken)
                           .ContinueWith(_ =>
                           {
                               try
                               {
                                   Model.BasicReject(deliveryTag, true);
                               }
                               catch (Exception ex)
                               {
                                   Logger.LogError(ex, "Message {EventName} reject error. [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey}", eventName, consumerTag, exchange, routingKey);
                               }
                           });
            }
        }

        Model.BasicReject(deliveryTag, false);
        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static bool CheckShouldRequeue(MessageRequeuePolicy policy, bool redelivered)
    {
        return policy switch
        {
            MessageRequeuePolicy.Never => false,
            MessageRequeuePolicy.Once => redelivered == false,
            _ => true,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static bool TryGetEventName(IDictionary<string, object> headers, ref string eventName)
    {
        if (headers.TryGetValue(RabbitMQOptions.EventNameHeaderKey, out var eventNameValue)
            && eventNameValue is byte[] eventNameBytes)
        {
            eventName = Encoding.UTF8.GetString(eventNameBytes);
            return true;
        }
        return false;
    }

    protected async Task ConsumeMessageAsync(ConsumeDescriptor consumeDescriptor, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, ReadOnlyMemory<byte> body)
    {
        var eventName = consumeDescriptor.EventName;
        Logger.LogDebug("Start consume message {EventName}. From [{ConsumerTag}] by {Exchange} -> {RoutingKey}.", eventName, consumerTag, exchange, routingKey);

        var invokerDescriptors = consumeDescriptor.InvokerDescriptors;
        try
        {
            await using var serviceScope = ServiceScopeFactory.CreateAsyncScope();
            var serviceProvider = serviceScope.ServiceProvider;
            if (consumeDescriptor.SingleWorkflowEventInvoker)
            {
                await InvokeHandlerAsync(body, serviceProvider, invokerDescriptors[0], ObjectSerializer, RunningCancellationToken);
            }
            else
            {
                var tasks = invokerDescriptors.Select([StackTraceHidden][DebuggerStepThrough] (invokerDescriptor) =>
                {
                    return InvokeHandlerAsync(body, serviceProvider, invokerDescriptor, ObjectSerializer, RunningCancellationToken);
                }).ToList();

                await Task.WhenAll(tasks);
            }
            Model.BasicAck(deliveryTag, false);
        }
        catch (Exception ex)
        {
            var requeue = CheckShouldRequeue(consumeDescriptor.RequeuePolicy, redelivered);

            Logger.LogError(ex, "Consume message error at {WorkflowName}.{EventName} [Requeue:{Requeue}]", invokerDescriptors[0].WorkflowName, invokerDescriptors[0].EventName, requeue);

            if (requeue)
            {
                await Task.Delay(consumeDescriptor.RequeueDelay, RunningCancellationToken);
            }

            Model.BasicReject(deliveryTag, requeue);
        }
    }

    /// <inheritdoc cref="HandleBasicDeliver(string, ulong, bool, string, string, IBasicProperties, ReadOnlyMemory{byte})"/>
    protected abstract Task InternalHandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body);

    #endregion Protected 方法

    #region Private 方法

    [StackTraceHidden]
    [DebuggerStepThrough]
    private async Task InnerInvokeAsync(WorkflowEventInvokerDescriptor invokerDescriptor, object message, object handler, CancellationToken cancellationToken)
    {
        Exception? exception = null;
        try
        {
            await invokerDescriptor.HandlerInvokeDelegate(handler, message, cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            DiagnosticSource.MessageHandleFinished(message, exception);
        }
    }

    [StackTraceHidden]
    [DebuggerStepThrough]
    private async Task InnerInvokeWithActivityAsync(WorkflowEventInvokerDescriptor invokerDescriptor, object message, object handler, Activity activity, CancellationToken cancellationToken)
    {
        Exception? exception = null;
        try
        {
            await invokerDescriptor.HandlerInvokeDelegate(handler, message, cancellationToken);
        }
        catch (Exception ex)
        {
            activity.RecordException(ex);
            exception = ex;
            throw;
        }
        finally
        {
            DiagnosticSource.MessageHandleFinished(message, exception);
            activity.Dispose();
        }
    }

    private Task InvokeHandlerAsync(ReadOnlyMemory<byte> rawMessageData,
                                    IServiceProvider serviceProvider,
                                    WorkflowEventInvokerDescriptor invokerDescriptor,
                                    IObjectSerializer objectSerializer,
                                    CancellationToken cancellationToken)
    {
        Activity? activity = null;
        var message = objectSerializer.Deserialize(rawMessageData.Span, invokerDescriptor.MessageType)!;
        if (message is IWorkflowContextCarrier<IWorkflowContext> contextCarrier)
        {
            var parentTraceContext = contextCarrier.Context.GetValue<TracingContext?>(FluentWorkflowConstants.ContextKeys.ParentTraceContext);
            if (parentTraceContext.HasValue) //恢复并移除上下文中的追踪上下文数据
            {
                var activityContext = parentTraceContext.Value.RestoreActivityContext(true);

                activity = ConsumerActivitySource.CreateActivity($"ConsumeWorkflowEventMessage - {invokerDescriptor.EventName}", ActivityKind.Consumer, activityContext);
                activity?.AddBaggages(parentTraceContext.Value.Baggage);
                activity?.Start();

                contextCarrier.Context.SetValue<TracingContext?>(FluentWorkflowConstants.ContextKeys.ParentTraceContext, null);
            }
        }

        DiagnosticSource.MessageReceived(message);

        var handler = serviceProvider.GetRequiredService(invokerDescriptor.TargetHandlerType);
        return activity is null
               ? InnerInvokeAsync(invokerDescriptor, message, handler, cancellationToken)
               : InnerInvokeWithActivityAsync(invokerDescriptor, message, handler, activity, cancellationToken);
    }

    #endregion Private 方法
}
