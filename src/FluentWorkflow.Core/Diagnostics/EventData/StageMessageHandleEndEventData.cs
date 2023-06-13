using FluentWorkflow.Interface;

namespace FluentWorkflow.Diagnostics.EventData;

/// <summary>
/// 事件数据 - <see cref="DiagnosticConstants.StageMessageHandleEnd"/>
/// </summary>
public class StageMessageHandleEndEventData
{
    #region Public 属性

    /// <summary>
    /// 处理过程中的异常
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// 是否通过
    /// </summary>
    public bool IsThrough { get; }

    /// <inheritdoc cref="IWorkflowStageMessage"/>
    public IWorkflowStageMessage StageMessage { get; set; }

    #endregion Public 属性

    #region Internal 构造函数

    internal StageMessageHandleEndEventData(IWorkflowStageMessage stageMessage, bool isThrough, Exception? exception)
    {
        StageMessage = stageMessage ?? throw new ArgumentNullException(nameof(stageMessage));
        IsThrough = isThrough;
        Exception = exception;
    }

    #endregion Internal 构造函数
}
