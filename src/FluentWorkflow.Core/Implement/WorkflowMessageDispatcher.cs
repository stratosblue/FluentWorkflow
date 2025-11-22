using FluentWorkflow.Abstractions;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Extensions;
using FluentWorkflow.MessageDispatch;
using FluentWorkflow.Scheduler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FluentWorkflow;

/// <summary>
/// <inheritdoc cref="IWorkflowMessageDispatcher"/>
/// </summary>
/// <inheritdoc cref="WorkflowMessageDispatcher"/>
public abstract class WorkflowMessageDispatcher(IWorkflowDiagnosticSource diagnosticSource, ILogger? logger)
    : IWorkflowMessageDispatcher
{
    #region Protected 字段

    /// <inheritdoc cref="ILogger"/>
    protected readonly ILogger Logger = logger ?? NullLogger.Instance;

    #endregion Protected 字段

    #region Public 方法

    /// <inheritdoc/>
    public virtual Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        diagnosticSource.MessagePublish(message);

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Publish [{EventName}] message - {{{Id}}}[{Message}].", TMessage.EventName, message.Id, message);
        }

        message.Context.State.StageState = WorkflowStageState.Scheduled;
        message.Context.AppendForwarded();

        return Task.CompletedTask;
    }

    #endregion Public 方法
}
