using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Diagnostics.EventData;

/// <summary>
/// 事件数据 - <see cref="DiagnosticConstants.StageMessageHandleStart"/>
/// </summary>
public class StageMessageHandleStartEventData
{
    #region Public 属性

    /// <inheritdoc cref="IWorkflowStageMessage"/>
    public IWorkflowStageMessage StageMessage { get; set; }

    #endregion Public 属性

    #region Internal 构造函数

    internal StageMessageHandleStartEventData(IWorkflowStageMessage stageMessage)
    {
        StageMessage = stageMessage ?? throw new ArgumentNullException(nameof(stageMessage));
    }

    #endregion Internal 构造函数
}
