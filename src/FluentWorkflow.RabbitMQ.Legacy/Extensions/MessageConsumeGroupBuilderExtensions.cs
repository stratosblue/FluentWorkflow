using System.Collections.Immutable;
using FluentWorkflow.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    /// <param name="durable"></param>
    /// <returns></returns>
    public static void DeclareQosQueue(IModel channel, string queueName, Dictionary<string, object> queueArguments, ushort qos, bool durable)
    {
        channel.QueueDeclare(queue: queueName, durable: durable, exclusive: false, autoDelete: !durable, arguments: queueArguments!);
        channel.BasicQos(prefetchSize: 0, prefetchCount: qos, global: false);
    }

    /// <summary>
    /// 使用Qos信道初始化回调
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="qos"></param>
    /// <returns></returns>
    public static MessageConsumeGroupBuilder WithQosChannelInitialization(this MessageConsumeGroupBuilder builder, ushort qos)
    {
        builder.WithInitialization(CreateQosChannelAsync);
        return builder;

        async Task<MessageConsumeGroupInitializationResult> CreateQosChannelAsync(IServiceProvider serviceProvider,
                                                                                  string queueName,
                                                                                  Dictionary<string, object> queueArguments,
                                                                                  MessageConsumeGroup messageConsumeGroup,
                                                                                  ImmutableDictionary<string, ImmutableArray<WorkflowEventInvokerDescriptor>> eventSubscribeDescriptors,
                                                                                  CancellationToken cancellationToken)
        {
            var connectionProvider = serviceProvider.GetRequiredService<IRabbitMQConnectionProvider>();
            var connection = await connectionProvider.GetAsync(cancellationToken);
            try
            {
                var channel = connection.CreateModel();

#pragma warning disable CS0618 // 类型或成员已过时
                var durable = serviceProvider.GetService<IOptions<RabbitMQOptions>>()?.Value?.Durable ?? true;
#pragma warning restore CS0618 // 类型或成员已过时

                DeclareQosQueue(channel, queueName, queueArguments, qos, durable);
                var channelScope = new ChannelScope(channel, connection.Dispose);
                return MessageConsumeGroupInitializationResult.CreateByChannelScope(channelScope);
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
