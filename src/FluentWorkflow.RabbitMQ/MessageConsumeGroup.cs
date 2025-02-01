using System.Collections.Immutable;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 信道创建委托
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="queueName"></param>
/// <param name="queueArguments"></param>
/// <param name="messageConsumeGroup"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task<ChannelScope> CreateChannelDelegate(IServiceProvider serviceProvider,
                                                         string queueName,
                                                         Dictionary<string, object> queueArguments,
                                                         MessageConsumeGroup messageConsumeGroup,
                                                         CancellationToken cancellationToken);

/// <summary>
/// 消息消费组
/// </summary>
/// <param name="Name"></param>
/// <param name="Messages"></param>
/// <param name="ChannelFactory"></param>
/// <param name="MessageHandleOptions"></param>
public record class MessageConsumeGroup(string Name,
                                        ImmutableDictionary<string, Type> Messages,
                                        CreateChannelDelegate ChannelFactory,
                                        MessageHandleOptions MessageHandleOptions);
