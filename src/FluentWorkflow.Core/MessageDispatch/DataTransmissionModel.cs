using FluentWorkflow.Abstractions;
using FluentWorkflow.Tracing;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 数据传输模型
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IDataTransmissionModel<out TMessage>
{
    #region Public 属性

    /// <summary>
    /// 事件名称 (该值在2.1版本之前不存在)
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public TMessage Message { get; }

    /// <summary>
    /// 追踪上下文
    /// </summary>
    public TracingContext? TracingContext { get; }

    #endregion Public 属性
}

/// <summary>
/// 数据传输模型
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <param name="EventName"></param>
/// <param name="Message"></param>
/// <param name="TracingContext"></param>
public record class DataTransmissionModel<TMessage>(string EventName, TMessage Message, TracingContext? TracingContext)
    : IDataTransmissionModel<TMessage>
    where TMessage : IWorkflowMessage, IEventNameDeclaration
{
}
