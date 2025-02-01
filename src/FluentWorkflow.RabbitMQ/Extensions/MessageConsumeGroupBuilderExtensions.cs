using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 消息消费组构建拓展
/// </summary>
public static class MessageConsumeGroupBuildExtensions
{
    #region Public 方法

    /// <summary>
    /// 定义一个Qos队列
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="queueName"></param>
    /// <param name="queueArguments"></param>
    /// <param name="qos"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task DeclareQosQueueAsync(IChannel channel, string queueName, Dictionary<string, object> queueArguments, ushort qos, CancellationToken cancellationToken)
    {
        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments!, noWait: false, cancellationToken: cancellationToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: qos, global: false, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 使用Qos信道工厂
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="qos"></param>
    /// <returns></returns>
    public static MessageConsumeGroupBuilder WithQosChannelFactory(this MessageConsumeGroupBuilder builder, ushort qos)
    {
        builder.WithChannelFactory(CreateQosChannelAsync);
        return builder;

        async Task<ChannelScope> CreateQosChannelAsync(IServiceProvider serviceProvider,
                                                       string queueName,
                                                       Dictionary<string, object> queueArguments,
                                                       MessageConsumeGroup messageConsumeGroup,
                                                       CancellationToken cancellationToken)
        {
            var connectionProvider = serviceProvider.GetRequiredService<IRabbitMQConnectionProvider>();
            var connection = await connectionProvider.GetAsync(cancellationToken);
            try
            {
                var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await DeclareQosQueueAsync(channel, queueName, queueArguments, qos, cancellationToken);
                return new ChannelScope(channel, connection.Dispose);
            }
            catch
            {
                //在初始化失败时释放了其它地方使用的链接可能有问题，但目前影响不大
                connection.Dispose();
                throw;
            }
        }
    }

    #endregion Public 方法
}
