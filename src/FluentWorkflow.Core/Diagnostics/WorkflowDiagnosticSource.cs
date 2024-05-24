using System.Diagnostics;
using FluentWorkflow.Diagnostics.EventData;
using FluentWorkflow.Interface;

namespace FluentWorkflow.Diagnostics;

/// <summary>
/// Workflow 诊断源
/// </summary>
internal sealed class WorkflowDiagnosticSource
    : DiagnosticListener, IWorkflowDiagnosticSource
{
    #region Public 构造函数

    /// <inheritdoc cref="WorkflowDiagnosticSource"/>
    public WorkflowDiagnosticSource() : base(DiagnosticConstants.DiagnosticName)
    {
    }

    /// <inheritdoc/>
    public void MessageHandleFinished(object message, Exception? exception)
    {
        if (IsEnabled(DiagnosticConstants.MessageHandleFinished))
        {
            Write(DiagnosticConstants.MessageHandleFinished, new MessageHandleFinishedEventData(message, exception));
        }
    }

    /// <inheritdoc/>
    public void MessagePublish(IWorkflowMessage message)
    {
        if (IsEnabled(DiagnosticConstants.MessagePublish))
        {
            Write(DiagnosticConstants.MessagePublish, new MessagePublishEventData(message));
        }
    }

    /// <inheritdoc/>
    public void MessageReceived(object message)
    {
        if (IsEnabled(DiagnosticConstants.MessageReceived))
        {
            Write(DiagnosticConstants.MessageReceived, new MessageReceivedEventData(message));
        }
    }

    /// <inheritdoc/>
    public void StageMessageHandleEnd(IWorkflowStageMessage stageMessage, bool isThrough, Exception? exception = null)
    {
        if (IsEnabled(DiagnosticConstants.StageMessageHandleEnd))
        {
            Write(DiagnosticConstants.StageMessageHandleEnd, new StageMessageHandleEndEventData(stageMessage, isThrough, exception));
        }
    }

    /// <inheritdoc/>
    public void StageMessageHandleStart(IWorkflowStageMessage stageMessage)
    {
        if (IsEnabled(DiagnosticConstants.StageMessageHandleStart))
        {
            Write(DiagnosticConstants.StageMessageHandleStart, new StageMessageHandleStartEventData(stageMessage));
        }
    }

    /// <inheritdoc/>
    public void WorkflowScheduleStart(IWorkflow workflow)
    {
        if (IsEnabled(DiagnosticConstants.WorkflowScheduleStart))
        {
            Write(DiagnosticConstants.WorkflowScheduleStart, new WorkflowScheduleStartEventData(workflow));
        }
    }

    /// <inheritdoc/>
    public void WorkflowStageCompleted(IWorkflowStageCompletedMessage completedMessage)
    {
        if (IsEnabled(DiagnosticConstants.WorkflowStageCompleted))
        {
            Write(DiagnosticConstants.WorkflowStageCompleted, new WorkflowStageCompletedEventData(completedMessage));
        }
    }

    #endregion Public 构造函数
}
