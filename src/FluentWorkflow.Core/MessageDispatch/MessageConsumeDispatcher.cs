using System.Collections.Immutable;
using System.Diagnostics;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Build;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 消息消费调度器
/// </summary>
public class MessageConsumeDispatcher : IMessageConsumeDispatcher
{
    #region Protected 属性

    /// <summary>
    /// 消费描述
    /// </summary>
    protected ImmutableDictionary<string, MessageConsumeDescriptor> ConsumeDescriptors { get; }

    /// <inheritdoc cref="IWorkflowDiagnosticSource"/>
    protected IWorkflowDiagnosticSource DiagnosticSource { get; }

    /// <inheritdoc cref="ILogger"/>
    protected ILogger Logger { get; }

    /// <inheritdoc cref="IServiceScopeFactory"/>
    protected IServiceScopeFactory ServiceScopeFactory { get; }

    #endregion Protected 属性

    #region Public 构造函数

    /// <inheritdoc cref="MessageConsumeDispatcher"/>
    public MessageConsumeDispatcher(IServiceScopeFactory serviceScopeFactory,
                                    IWorkflowDiagnosticSource diagnosticSource,
                                    WorkflowBuildStateCollection workflowBuildStates,
                                    ILogger<MessageConsumeDispatcher> logger)
    {
        ArgumentNullException.ThrowIfNull(workflowBuildStates);

        ServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        DiagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));

        ConsumeDescriptors = workflowBuildStates.SelectMany(static m => m)
                                                .GroupBy(static m => m.EventName)
                                                .ToImmutableDictionary(static m => m.Key, static m =>
                                                {
                                                    var invokerDescriptors = m.SelectMany(static m => m).ToImmutableArray();
                                                    return new MessageConsumeDescriptor(m.Key, invokerDescriptors);
                                                });

        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual Task DispatchAsync(string eventName, object message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (ConsumeDescriptors.TryGetValue(eventName, out var consumeDescriptor))
        {
            return ConsumeMessageAsync(eventName, message, consumeDescriptor, cancellationToken);
        }

        throw new MessageConsumerNotFoundException(eventName, message);
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 消费消息
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="message"></param>
    /// <param name="consumeDescriptor"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ConsumeMessageAsync(string eventName, object message, MessageConsumeDescriptor consumeDescriptor, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Start consume message {EventName} - {Message}.", eventName, message);

        Activity.Current?.AddEvent($"Invoke Message Handler - {eventName}");

        Exception? exception = null;
        try
        {
            var invokerDescriptors = consumeDescriptor.InvokerDescriptors;

            if (consumeDescriptor.SingleWorkflowEventInvoker)
            {
                await using var serviceScope = ServiceScopeFactory.CreateAsyncScope();
                var serviceProvider = serviceScope.ServiceProvider;
                await InnerInvokeAsync(message, invokerDescriptors[0], serviceProvider, cancellationToken);
            }
            else
            {
                var tasks = invokerDescriptors.Select([StackTraceHidden][DebuggerStepThrough] async (invokerDescriptor) =>
                {
                    await using var serviceScope = ServiceScopeFactory.CreateAsyncScope();
                    var serviceProvider = serviceScope.ServiceProvider;
                    await InnerInvokeAsync(message, invokerDescriptor, serviceProvider, cancellationToken);
                }).ToList();

                await Task.WhenAll(tasks);
            }
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
        Logger.LogDebug("Consume message {EventName} - {Message} successfully.", eventName, message);
    }

    #endregion Protected 方法

    #region Private 方法

    [StackTraceHidden]
    [DebuggerStepThrough]
    private static Task InnerInvokeAsync(object message, WorkflowEventInvokerDescriptor invokerDescriptor, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService(invokerDescriptor.TargetHandlerType);

        return invokerDescriptor.HandlerInvokeDelegate(handler, message, cancellationToken);
    }

    #endregion Private 方法
}
