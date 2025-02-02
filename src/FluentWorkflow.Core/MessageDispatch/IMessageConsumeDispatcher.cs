using System.ComponentModel;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 消息消费调度器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public interface IMessageConsumeDispatcher
{
    /// <summary>
    /// 调度消息
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task DispatchAsync(string eventName, object message, CancellationToken cancellationToken);
}
