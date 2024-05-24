using System.ComponentModel;
using FluentWorkflow.Interface;

namespace FluentWorkflow.Diagnostics;

/// <summary>
/// 诊断源
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWorkflowDiagnosticSource
{
    #region Public 方法

    /// <inheritdoc cref="DiagnosticConstants.MessagePublish"/>
    void MessagePublish(IWorkflowMessage message);

    /// <inheritdoc cref="DiagnosticConstants.MessageReceived"/>
    void MessageReceived(object message);

    /// <inheritdoc cref="DiagnosticConstants.StageMessageHandleEnd"/>
    void StageMessageHandleEnd(IWorkflowStageMessage stageMessage, bool isThrough, Exception? exception = null);

    /// <inheritdoc cref="DiagnosticConstants.StageMessageHandleStart"/>
    void StageMessageHandleStart(IWorkflowStageMessage stageMessage);

    /// <inheritdoc cref="DiagnosticConstants.WorkflowScheduleStart"/>
    void WorkflowScheduleStart(IWorkflow workflow);

    /// <inheritdoc cref="DiagnosticConstants.WorkflowStageCompleted"/>
    void WorkflowStageCompleted(IWorkflowStageCompletedMessage completedMessage);

    #endregion Public 方法
}
