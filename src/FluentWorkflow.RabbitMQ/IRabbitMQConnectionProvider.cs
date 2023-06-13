using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// RabbitMQ连接提供器
/// </summary>
public interface IRabbitMQConnectionProvider
{
    #region Public 方法

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IConnection> GetAsync(CancellationToken cancellationToken);

    #endregion Public 方法
}
