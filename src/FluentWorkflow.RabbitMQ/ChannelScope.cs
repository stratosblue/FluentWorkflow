using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// <see cref="IChannel"/> 域
/// </summary>
/// <param name="channel"></param>
/// <param name="disposeCallback"></param>
public class ChannelScope(IChannel channel, Action disposeCallback) : IDisposable
{
    /// <inheritdoc cref="IChannel"/>
    public IChannel Channel { get; } = channel;

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        Channel.Dispose();
        disposeCallback?.Invoke();
    }
}
