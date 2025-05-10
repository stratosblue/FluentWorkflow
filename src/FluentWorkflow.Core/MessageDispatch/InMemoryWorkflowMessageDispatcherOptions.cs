using System.ComponentModel;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 基于内存的 <inheritdoc cref="IWorkflowMessageDispatcher"/> 分发选项
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public class InMemoryWorkflowMessageDispatcherOptions
{
    #region Public 委托

    /// <summary>
    /// 消费消息时的回调
    /// </summary>
    /// <param name="transmissionModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>是否需要消费（返回 <see langword="false"/> 时不会进行消费）</returns>
    public delegate Task<bool> ConsumeMessageCallbackDelegate(IDataTransmissionModel<object> transmissionModel, CancellationToken cancellationToken);

    /// <summary>
    /// 出现当前未订阅消息时的回调
    /// </summary>
    /// <param name="transmissionModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>是否已处理（返回 <see langword="false"/> 时会抛出异常）</returns>
    public delegate Task<bool> UnsubscribedMessageCallbackDelegate(IDataTransmissionModel<object> transmissionModel, CancellationToken cancellationToken);

    #endregion Public 委托

    #region Public 属性

    /// <summary>
    /// 消费消息时的回调
    /// </summary>
    public UnsubscribedMessageCallbackDelegate? ConsumeMessageCallback { get; set; }

    /// <summary>
    /// 出现当前未订阅消息时的回调
    /// </summary>
    public UnsubscribedMessageCallbackDelegate? UnsubscribedMessageCallback { get; set; }

    #endregion Public 属性
}
