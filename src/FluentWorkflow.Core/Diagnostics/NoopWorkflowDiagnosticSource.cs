using FluentWorkflow.Interface;

namespace FluentWorkflow.Diagnostics;

/// <summary>
/// 无操作的 <inheritdoc cref="IWorkflowDiagnosticSource"/>
/// </summary>
internal sealed class NoopWorkflowDiagnosticSource
    : IWorkflowDiagnosticSource
{
    #region Public 方法

    /// <inheritdoc/>
    public void MessagePublish(IWorkflowMessage message)
    { }

    /// <inheritdoc/>
    public void MessageReceived(object message)
    { }

    /// <inheritdoc/>
    public void StageMessageHandleEnd(IWorkflowStageMessage stageMessage, bool isThrough, Exception? exception = null)
    { }

    /// <inheritdoc/>
    public void StageMessageHandleStart(IWorkflowStageMessage stageMessage)
    { }

    /// <inheritdoc/>
    public void WorkflowScheduleStart(IWorkflow workflow)
    { }

    /// <inheritdoc/>
    public void WorkflowStageCompleted(IWorkflowStageCompletedMessage completedMessage)
    { }

    #endregion Public 方法
}
