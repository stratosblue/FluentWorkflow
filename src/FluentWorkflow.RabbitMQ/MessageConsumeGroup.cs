using System.Collections.Immutable;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 消息消费组初始化委托
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="queueName"></param>
/// <param name="queueArguments"></param>
/// <param name="messageConsumeGroup"></param>
/// <param name="eventSubscribeDescriptors"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task<MessageConsumeGroupInitializationResult> MessageConsumeGroupInitializationDelegate(IServiceProvider serviceProvider,
                                                                                                        string queueName,
                                                                                                        Dictionary<string, object> queueArguments,
                                                                                                        MessageConsumeGroup messageConsumeGroup,
                                                                                                        ImmutableDictionary<string, ImmutableArray<WorkflowEventInvokerDescriptor>> eventSubscribeDescriptors,
                                                                                                        CancellationToken cancellationToken);

/// <summary>
/// 消息消费组初始化结果
/// </summary>
public record class MessageConsumeGroupInitializationResult
{
    /// <summary>
    /// 已自定义处理 (该分组消息已由用户自定义处理，框架不再执行相关的后续逻辑)
    /// </summary>
    public bool CustomHandled { get; }

    /// <summary>
    /// 消息消费组信道域
    /// </summary>
    public ChannelScope? ChannelScope { get; }

    private MessageConsumeGroupInitializationResult(bool customHandled, ChannelScope? channelScope)
    {
        CustomHandled = customHandled;
        ChannelScope = channelScope;
    }

    /// <summary>
    /// 创建一个自定义处理的初始化结果
    /// </summary>
    /// <returns></returns>
    public static MessageConsumeGroupInitializationResult CreateCustomHandled()
    {
        return new MessageConsumeGroupInitializationResult(true, null);
    }

    /// <summary>
    /// 创建一个非自定义处理的初始化结果
    /// </summary>
    /// <param name="channelScope"></param>
    /// <returns></returns>
    public static MessageConsumeGroupInitializationResult CreateByChannelScope(ChannelScope channelScope)
    {
        ArgumentNullException.ThrowIfNull(channelScope);

        return new MessageConsumeGroupInitializationResult(false, channelScope);
    }
}

/// <summary>
/// 消息消费组
/// </summary>
/// <param name="Name">组名称</param>
/// <param name="Messages">消息列表</param>
/// <param name="GroupInitializationCallback">消费组初始化回调</param>
/// <param name="MessageHandleOptions">消息处理选项</param>
public record class MessageConsumeGroup(string Name,
                                        ImmutableDictionary<string, Type> Messages,
                                        MessageConsumeGroupInitializationDelegate GroupInitializationCallback,
                                        MessageHandleOptions MessageHandleOptions);
