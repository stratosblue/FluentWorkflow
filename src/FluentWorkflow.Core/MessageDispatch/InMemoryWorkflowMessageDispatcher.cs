using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Build;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Tracing;
using FluentWorkflow.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 基于内存的 <inheritdoc cref="IWorkflowMessageDispatcher"/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public class InMemoryWorkflowMessageDispatcher : IWorkflowMessageDispatcher
{
    #region Protected 属性

    /// <summary>
    /// 工作流程事件执行程序描述符订阅列表
    /// </summary>
    protected ImmutableDictionary<string, ImmutableArray<WorkflowEventInvokerDescriptor>> EventSubscribeDescriptors { get; }

    /// <inheritdoc cref="ILogger"/>
    protected ILogger Logger { get; }

    /// <inheritdoc cref="IMessageConsumeDispatcher"/>
    protected IMessageConsumeDispatcher MessageConsumeDispatcher { get; }

    /// <inheritdoc cref="IObjectSerializer"/>
    protected IObjectSerializer ObjectSerializer { get; }

    /// <inheritdoc cref="InMemoryWorkflowMessageDispatcherOptions"/>
    protected InMemoryWorkflowMessageDispatcherOptions Options { get; }

    /// <inheritdoc cref="IServiceScopeFactory"/>
    protected IServiceScopeFactory ServiceScopeFactory { get; }

    #endregion Protected 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="InMemoryWorkflowMessageDispatcher"/>
    /// </summary>
    public InMemoryWorkflowMessageDispatcher(IServiceScopeFactory serviceScopeFactory,
                                             WorkflowBuildStateCollection workflowBuildStates,
                                             IObjectSerializer objectSerializer,
                                             IMessageConsumeDispatcher messageConsumeDispatcher,
                                             IOptions<InMemoryWorkflowMessageDispatcherOptions> options,
                                             ILogger<InMemoryWorkflowMessageDispatcher> logger)
    {
        ArgumentNullException.ThrowIfNull(workflowBuildStates);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(objectSerializer);
        ArgumentNullException.ThrowIfNull(messageConsumeDispatcher);
        ArgumentNullException.ThrowIfNull(options.Value);
        ArgumentNullException.ThrowIfNull(logger);

        ServiceScopeFactory = serviceScopeFactory;
        ObjectSerializer = objectSerializer;
        MessageConsumeDispatcher = messageConsumeDispatcher;
        Options = options.Value;
        EventSubscribeDescriptors = workflowBuildStates.GetEventInvokeMap();
        Logger = logger;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        ArgumentNullException.ThrowIfNull(message);

        var cloneMessage = CloneMessage(message);

        if (!EventSubscribeDescriptors.ContainsKey(TMessage.EventName))
        {
            if (await OnUnsubscribedMessageAsync(cloneMessage, cancellationToken))
            {
                return;
            }
            throw new InvalidOperationException($"Not found event subscriber for {{{cloneMessage.Id}}}\"{TMessage.EventName}\".");
        }

        var tracingContext = TracingContext.TryCapture();

        using var asyncFlowControl = ExecutionContext.SuppressFlow();

        //默认异步执行
        _ = Task.Run(async () =>
        {
            if (await OnConsumeMessageAsync(message, tracingContext, CancellationToken.None))
            {
                await ConsumeDispatchAsync(message, tracingContext, CancellationToken.None);
            }
        }, CancellationToken.None);
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 克隆消息，以避免对其的修改影响执行
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <returns></returns>
    protected TMessage CloneMessage<TMessage>(TMessage message)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        var serializedMessage = ObjectSerializer.SerializeToBytes(message);
        return ObjectSerializer.Deserialize<TMessage>(serializedMessage)!;
    }

    /// <summary>
    /// 消费调度
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="tracingContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task ConsumeDispatchAsync<TMessage>(TMessage message, TracingContext? tracingContext, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        Activity? activity = null;
        try
        {
            if (tracingContext != null)
            {
                var activityContext = tracingContext.Value.RestoreActivityContext(true);

                activity = ActivitySourceDefine.ActivitySource.StartActivity($"ConsumeWorkflowEventMessage {TMessage.EventName}", ActivityKind.Consumer, activityContext);

                activity?.AddBaggages(tracingContext.Value.Baggage);
            }

            await MessageConsumeDispatcher.DispatchAsync(TMessage.EventName, message, cancellationToken);
        }
        catch (Exception ex)
        {
            if (Logger.IsEnabled(LogLevel.Error))
            {
                Logger.LogError(ex, "Error at handle message - {{{Id}}}\"{EventName}\"", message.Id, TMessage.EventName);
            }
        }
        finally
        {
            activity?.Dispose();
        }
    }

    /// <summary>
    /// 在消费消息时执行的方法
    /// </summary>
    /// <param name="message"></param>
    /// <param name="tracingContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>是否需要消费（返回 <see langword="false"/> 时不会进行消费）</returns>
    protected virtual Task<bool> OnConsumeMessageAsync<TMessage>(TMessage message, TracingContext? tracingContext, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        if (Options.ConsumeMessageCallback is { } consumeMessageCallback)
        {
            var dataTransmissionModel = new DataTransmissionModel<TMessage>(TMessage.EventName, message, tracingContext);
            return consumeMessageCallback(dataTransmissionModel, cancellationToken);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    /// 在出现未订阅的消息时执行的方法
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回 <see langword="true"/> 则不抛出异常</returns>
    protected virtual Task<bool> OnUnsubscribedMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        if (Options.UnsubscribedMessageCallback is { } unsubscribedMessageCallback)
        {
            var dataTransmissionModel = new DataTransmissionModel<TMessage>(TMessage.EventName, message, TracingContext.TryCapture());
            return unsubscribedMessageCallback(dataTransmissionModel, cancellationToken);
        }

        return Task.FromResult(false);
    }

    #endregion Protected 方法
}
