﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using FluentWorkflow.Build;
using FluentWorkflow.Interface;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

internal abstract class EventMessageBasicConsumer : AsyncDefaultBasicConsumer
{
    #region Protected 字段

    protected readonly JsonSerializerOptions JsonSerializerOptions = SystemTextJsonObjectSerializer.JsonSerializerOptions;

    protected readonly ILogger Logger;

    protected readonly CancellationToken RunningCancellationToken;

    protected readonly IServiceScopeFactory ServiceScopeFactory;

    #endregion Protected 字段

    #region Public 构造函数

    public EventMessageBasicConsumer(IModel model, IServiceScopeFactory serviceScopeFactory, ILogger logger, CancellationToken runningCancellationToken) : base(model)
    {
        ServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        RunningCancellationToken = runningCancellationToken;
    }

    #endregion Public 构造函数

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
            if (invokerDescriptors.Length == 1)
            {
                await InvokeHandlerAsync(body, serviceProvider, invokerDescriptors[0], JsonSerializerOptions, RunningCancellationToken);
            }
            else
            {
                var tasks = invokerDescriptors.Select([StackTraceHidden][DebuggerStepThrough] (invokerDescriptor) =>
                {
                    return InvokeHandlerAsync(body, serviceProvider, invokerDescriptor, JsonSerializerOptions, RunningCancellationToken);
                }).ToArray();

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

    #endregion Protected 方法

    #region Private 方法

    private static Task InvokeHandlerAsync(ReadOnlyMemory<byte> rawMessageData,
                                           IServiceProvider serviceProvider,
                                           WorkflowEventInvokerDescriptor invokerDescriptor,
                                           JsonSerializerOptions jsonSerializerOptions,
                                           CancellationToken cancellationToken)
    {
        Activity? activity = null;
        var message = JsonSerializer.Deserialize(rawMessageData.Span, invokerDescriptor.MessageType, jsonSerializerOptions)!;
        if (message is IWorkflowContextCarrier<IWorkflowContext> contextCarrier)
        {
            var parentTraceContextData = contextCarrier.Context.GetValue(FluentWorkflowConstants.ContextKeys.ParentTraceContext);
            if (!string.IsNullOrWhiteSpace(parentTraceContextData)) //恢复并移除上下文中的追踪上下文数据
            {
                var tracingContext = TracingContext.Deserialize(parentTraceContextData);
                var activityContext = tracingContext.RestoreActivityContext(true);

                activity = ConsumerActivitySource.StartActivity("ConsumeEventMessage", ActivityKind.Consumer, activityContext);

                contextCarrier.Context.SetValue(FluentWorkflowConstants.ContextKeys.ParentTraceContext, null);
            }
        }
        var handler = serviceProvider.GetRequiredService(invokerDescriptor.TargetType);
        return activity is null
               ? invokerDescriptor.HandlerInvokeDelegate(handler, message, cancellationToken)
               : InvokeWithActivityAsync(invokerDescriptor, message, handler, activity, cancellationToken);
    }

    [StackTraceHidden]
    [DebuggerStepThrough]
    private static async Task InvokeWithActivityAsync(WorkflowEventInvokerDescriptor invokerDescriptor, object message, object handler, Activity activity, CancellationToken cancellationToken)
    {
        try
        {
            await invokerDescriptor.HandlerInvokeDelegate(handler, message, cancellationToken);
        }
        catch (Exception ex)
        {
            activity.RecordException(ex);
            throw;
        }
        finally
        {
            activity.Dispose();
        }
    }

    #endregion Private 方法
}
