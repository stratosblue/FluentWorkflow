using FluentWorkflow.Interface;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// RabbitMQ配置
/// </summary>
public class RabbitMQOptions
{
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
    /// 连接工厂，设置此值时会忽略 <see cref="Uri"/>
    /// </summary>
    public IAsyncConnectionFactory? ConnectionFactory { get; set; }

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
    /// 使用的交换机名称
    /// TODO 单元测试
    /// </summary>
    public string? ExchangeName { get; set; } = DefaultExchangeName;

    /// <summary>
    /// 全局Qos，默认为 0 不限制
    /// </summary>
    public ushort GlobalQos { get; set; } = 0;

    /// <summary>
    /// 队列 Arguments 设置委托
    /// </summary>
    public Action<string, IDictionary<string, object>>? QueueArgumentsSetup { get; set; }

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
