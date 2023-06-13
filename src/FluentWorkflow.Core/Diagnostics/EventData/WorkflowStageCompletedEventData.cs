using FluentWorkflow.Interface;

namespace FluentWorkflow.Diagnostics.EventData;

/// <summary>
/// 事件数据 - <see cref="DiagnosticConstants.WorkflowStageCompleted"/>
/// </summary>
public class WorkflowStageCompletedEventData
{
    #region Public 属性

    /// <inheritdoc cref="IWorkflowStageCompletedMessage"/>
    public IWorkflowStageCompletedMessage CompletedMessage { get; }

    #endregion Public 属性

    #region Internal 构造函数

    internal WorkflowStageCompletedEventData(IWorkflowStageCompletedMessage completedMessage)
    {
        CompletedMessage = completedMessage ?? throw new ArgumentNullException(nameof(completedMessage));
    }

    #endregion Internal 构造函数
}
