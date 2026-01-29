using System.ComponentModel;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 消息消费调度器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public interface IMessageConsumeDispatcher
{
    #region Public 方法

    /// <summary>
    /// 调度消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="message">消息</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task DispatchAsync(string eventName, object message, CancellationToken cancellationToken);

    #endregion Public 方法
}
