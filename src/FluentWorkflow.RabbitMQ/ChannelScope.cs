using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// <see cref="IChannel"/> 域
/// </summary>
/// <param name="channel"></param>
/// <param name="asyncDisposeCallback"></param>
public sealed class ChannelScope(IChannel channel, Func<ValueTask> asyncDisposeCallback) : IAsyncDisposable
{
    #region Public 属性

    /// <summary>
    /// 当前信道域对应的 <see cref="IChannel"/>
    /// </summary>
    public IChannel Channel { get; } = channel ?? throw new ArgumentNullException(nameof(channel));

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await Channel.DisposeAsync();
        await asyncDisposeCallback();
    }

    #endregion Public 方法
}
