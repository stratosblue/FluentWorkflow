namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程完成消息
/// </summary>
public interface IWorkflowFinishedMessage
    : IWorkflowMessage
    , IWorkflowContextCarrier<IWorkflowContext>
    , IWorkflowNameDeclaration
    , IEventNameDeclaration
{
    #region Public 属性

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; }

    #endregion Public 属性
}
