using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// RabbitMQ <see cref="IChannel"/> 池
/// </summary>
public interface IRabbitMQChannelPool
{
    #region Public 方法

    /// <summary>
    /// 获取 <see cref="IChannel"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<IChannel> RentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 归还 <paramref name="channel"/>
    /// </summary>
    /// <param name="channel"></param>
    void Return(IChannel channel);

    #endregion Public 方法
}
