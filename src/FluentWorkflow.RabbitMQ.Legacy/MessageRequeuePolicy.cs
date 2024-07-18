namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 消息重入队列策略
/// </summary>
public enum MessageRequeuePolicy : byte
{
    /// <summary>
    /// 默认
    /// </summary>
    Default = 0,

    /// <summary>
    /// 不重入
    /// </summary>
    Never = 1,

    /// <summary>
    /// 一次
    /// </summary>
    Once = 2,

    /// <summary>
    /// 无限次
    /// </summary>
    Unlimited = 3,
}
