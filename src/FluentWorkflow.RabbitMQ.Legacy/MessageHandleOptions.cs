namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 消息处理选项
/// </summary>
public class MessageHandleOptions
{
    #region Public 属性

    /// <summary>
    /// 消息重入队列的延时
    /// </summary>
    public TimeSpan RequeueDelay { get; set; } = RabbitMQOptions.MessageRequeueDelay;

    /// <summary>
    /// 失败消息重入队列的策略
    /// </summary>
    public MessageRequeuePolicy RequeuePolicy { get; set; } = MessageRequeuePolicy.Default;

    #endregion Public 属性
}
