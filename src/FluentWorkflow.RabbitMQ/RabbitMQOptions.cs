using FluentWorkflow.Interface;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// RabbitMQ配置
/// </summary>
public class RabbitMQOptions
{
    #region Public 委托

    /// <summary>
    /// 队列 Arguments 设置委托
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="arguments"></param>

    public delegate void QueueArgumentsSetupDelegate(string queueName, IDictionary<string, object> arguments);

    #endregion Public 委托

    #region Public 字段

    /// <summary>
    /// 默认交换机名称
    /// </summary>
    public const string DefaultExchangeName = "fwf.exchanger.default";

    /// <summary>
    /// EventName在Header中的Key名称
    /// </summary>
    public const string EventNameHeaderKey = "fwf.message.eventname";

    /// <summary>
    /// 工作流程ID在Header中的Key名称
    /// </summary>
    public const string WorkflowIdHeaderKey = "fwf.workflow.id";

    /// <summary>
    /// 默认消息重入队列的延时
    /// </summary>
    public static readonly TimeSpan MessageRequeueDelay = TimeSpan.FromSeconds(5);

    #endregion Public 字段

    #region Public 属性

    /// <summary>
    /// <see cref="IChannel"/> 最大池化数量
    /// </summary>
    public uint ChannelPoolMaxSize { get; set; } = (uint)Environment.ProcessorCount;

    /// <summary>
    /// 连接工厂，设置此值时会忽略 <see cref="Uri"/><br/>
    /// 注意:<br/>
    /// 需要支持自动恢复，即设置 <see cref="ConnectionFactory.AutomaticRecoveryEnabled"/> 和 <see cref="ConnectionFactory.TopologyRecoveryEnabled"/> 为 <see langword="true"/><br/>
    /// </summary>
    public IConnectionFactory? ConnectionFactory { get; set; }

    /// <summary>
    /// 消费队列名称
    /// </summary>
    public string? ConsumeQueueName { get; set; }

    /// <summary>
    /// 消费队列名称规范化委托
    /// </summary>
    public Func<string, string?>? ConsumeQueueNameNormalizer { get; set; }

    /// <summary>
    /// 错误消息重入队列的延时
    /// </summary>
    public TimeSpan ErrorMessageRequeueDelay { get; set; } = RabbitMQOptions.MessageRequeueDelay;

    /// <summary>
    /// 错误消息重入队列的策略
    /// </summary>
    public MessageRequeuePolicy ErrorMessageRequeuePolicy { get; set; } = MessageRequeuePolicy.Default;

    /// <summary>
    /// 默认使用的交换机名称（如果注册了自定义的 <see cref="IRabbitMQExchangeSelector"/>，则此配置无效）
    /// </summary>
    public string? ExchangeName { get; set; } = DefaultExchangeName;

    /// <summary>
    /// 全局Qos，默认为 0 不限制
    /// </summary>
    public ushort GlobalQos { get; set; } = 0;

    /// <summary>
    /// 更倾向于使用单一链接<br/>
    /// 值为 <see langword="true"/> 时，会尽可能的使用单个 <see cref="IConnection"/>
    /// </summary>
    public bool PreferSingleConnection { get; set; } = false;

    /// <summary>
    /// 发送者确认 see: https://www.rabbitmq.com/docs/confirms
    /// </summary>
    public bool PublisherConfirms { get; set; } = true;

    /// <summary>
    /// 队列 Arguments 设置委托
    /// </summary>
    public QueueArgumentsSetupDelegate? QueueArgumentsSetup { get; set; }

    /// <summary>
    /// Uri
    /// </summary>
    public Uri? Uri { get; set; }

    #endregion Public 属性

    #region Internal 属性

    /// <summary>
    /// 消息处理设置
    /// </summary>
    internal Dictionary<string, MessageHandleOptions> MessageHandleOptions { get; } = new(StringComparer.OrdinalIgnoreCase);

    #endregion Internal 属性

    #region Public 方法

    /// <summary>
    /// 配置选项
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="setupAction"></param>
    public void Option<TMessage>(Action<MessageHandleOptions> setupAction)
        where TMessage : IWorkflowMessage, IEventNameDeclaration
    {
        if (!MessageHandleOptions.TryGetValue(TMessage.EventName, out var options))
        {
            options = new MessageHandleOptions();
            MessageHandleOptions.Add(TMessage.EventName, options);
        }
        setupAction(options);
    }

    #endregion Public 方法
}
