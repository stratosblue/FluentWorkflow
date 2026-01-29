namespace FluentWorkflow.Abstractions;

/// <summary>
/// 工作流程失败消息
/// </summary>
public interface IWorkflowFailureMessage : IWorkflowStageMessage, IEventNameDeclaration
{
    #region Public 属性

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 远程堆栈追踪
    /// </summary>
    public string? RemoteStackTrace { get; }

    #endregion Public 属性
}
