using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// <see cref="IModel"/> 域
/// </summary>
/// <param name="channel"></param>
/// <param name="disposeCallback"></param>
public sealed class ChannelScope(IModel channel, Action disposeCallback) : IDisposable
{
    #region Public 属性

    /// <summary>
    /// 当前信道域对应的 <see cref="IModel"/>
    /// </summary>
    public IModel Channel { get; } = channel ?? throw new ArgumentNullException(nameof(channel));

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
        Channel.Dispose();
        disposeCallback?.Invoke();
    }

    #endregion Public 方法
}
