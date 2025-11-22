using FluentWorkflow.Abstractions;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 工作流程消息分发器
/// </summary>
public interface IWorkflowMessageDispatcher
{
    #region Public 方法

    /// <summary>
    /// 分发消息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration;

    #endregion Public 方法
}
