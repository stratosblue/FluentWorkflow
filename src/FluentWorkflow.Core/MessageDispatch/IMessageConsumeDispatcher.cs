using System.ComponentModel;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 消息消费调度器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public interface IMessageConsumeDispatcher
{
    public Task DispatchAsync(string eventName, object message, CancellationToken cancellationToken);
}
