using FluentWorkflow.Diagnostics;
using FluentWorkflow.Extensions;
using FluentWorkflow.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FluentWorkflow;

/// <summary>
/// <inheritdoc cref="IWorkflowMessageDispatcher"/>
/// </summary>
public abstract class WorkflowMessageDispatcher : IWorkflowMessageDispatcher
{
    #region Protected 字段

    /// <inheritdoc cref="ILogger"/>
    protected readonly ILogger Logger;

    #endregion Protected 字段

    #region Private 字段

    private readonly IWorkflowDiagnosticSource _diagnosticSource;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowMessageDispatcher"/>
    public WorkflowMessageDispatcher(IWorkflowDiagnosticSource diagnosticSource, ILogger? logger)
    {
        _diagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
        Logger = logger ?? NullLogger.Instance;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        _diagnosticSource.MessagePublish(message);
        Logger.LogTrace("Publish [{EventName}] message - {{{Id}}}[{Message}].", TMessage.EventName, message.Id, message);

        message.Context.State.StageState = WorkflowStageState.Scheduled;
        message.Context.AppendForwarded();

        return Task.CompletedTask;
    }

    #endregion Public 方法
}
