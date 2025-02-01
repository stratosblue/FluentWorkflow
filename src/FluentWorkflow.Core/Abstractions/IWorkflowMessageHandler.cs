namespace FluentWorkflow.Abstractions;

/// <summary>
/// 工作流程消息处理器
/// </summary>
public interface IWorkflowMessageHandler<in TMessage>
    where TMessage : IWorkflowMessage
{
    #region Public 方法

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);

    #endregion Public 方法
}
