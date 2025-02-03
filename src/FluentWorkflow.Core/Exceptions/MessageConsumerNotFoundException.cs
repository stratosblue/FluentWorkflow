#pragma warning disable CS8618

using System.Runtime.Serialization;

namespace FluentWorkflow;

/// <summary>
/// 消息消费者未找到异常
/// </summary>
public class MessageConsumerNotFoundException : WorkflowException
{
    #region Public 属性

    /// <summary>
    /// 事件消息
    /// </summary>
    public object EventMessage { get; }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string EventName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="MessageConsumerNotFoundException"/>
    public MessageConsumerNotFoundException(string eventName, object eventMessage, string exceptionMessage) : this(eventName, eventMessage, exceptionMessage, null)
    {
    }

    /// <inheritdoc cref="MessageConsumerNotFoundException"/>
    public MessageConsumerNotFoundException(string eventName, object eventMessage, string? exceptionMessage = null, Exception? innerException = null) : base(exceptionMessage ?? $"Not found consumer for \"{eventName}\".", innerException)
    {
        EventName = eventName;
        EventMessage = eventMessage;
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="MessageConsumerNotFoundException"/>
    [Obsolete("see https://github.com/dotnet/docs/issues/34893")]
    protected MessageConsumerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数
}
