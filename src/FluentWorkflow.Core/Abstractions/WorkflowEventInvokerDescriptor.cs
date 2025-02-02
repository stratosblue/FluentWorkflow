using System.ComponentModel;
using FluentWorkflow.MessageDispatch;

namespace FluentWorkflow.Abstractions;

/// <summary>
/// 事件消息处理器执行委托
/// </summary>
/// <param name="instance"></param>
/// <param name="message"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task EventMessageHandlerInvokeDelegate(object instance, object message, CancellationToken cancellationToken = default);

/// <summary>
/// 工作流程事件执行程序描述符
/// </summary>
public class WorkflowEventInvokerDescriptor : IEquatable<WorkflowEventInvokerDescriptor>
{
    #region Public 属性

    /// <summary>
    /// 事件名称
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// 实例执行委托
    /// </summary>
    public EventMessageHandlerInvokeDelegate HandlerInvokeDelegate { get; }

    /// <summary>
    /// 事件消息类型
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// 目标处理器类型
    /// </summary>
    public Type TargetHandlerType { get; }

    /// <summary>
    /// 传输类型 - <see cref="IDataTransmissionModel{TMessage}"/>
    /// </summary>
    public Type TransmissionType { get; }

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public string WorkflowName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowEventInvokerDescriptor"/>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public WorkflowEventInvokerDescriptor(string workflowName,
                                          string eventName,
                                          Type messageType,
                                          Type targetHandlerType,
                                          Type transmissionType,
                                          EventMessageHandlerInvokeDelegate handlerInvokeDelegate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        WorkflowName = workflowName;
        EventName = eventName;
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        TargetHandlerType = targetHandlerType ?? throw new ArgumentNullException(nameof(targetHandlerType));
        TransmissionType = transmissionType ?? throw new ArgumentNullException(nameof(transmissionType));
        HandlerInvokeDelegate = handlerInvokeDelegate ?? throw new ArgumentNullException(nameof(handlerInvokeDelegate));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 创建一个 <typeparamref name="TMessage"/> 的 <see cref="WorkflowEventInvokerDescriptor"/>
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="workflowName"></param>
    /// <param name="targetHandlerType"></param>
    /// <param name="handlerInvokeDelegate"></param>
    /// <returns></returns>
    public static WorkflowEventInvokerDescriptor Create<TMessage>(string workflowName, Type targetHandlerType, EventMessageHandlerInvokeDelegate handlerInvokeDelegate)
        where TMessage : IWorkflowMessage, IEventNameDeclaration
    {
        return new WorkflowEventInvokerDescriptor(workflowName: workflowName,
                                                  eventName: TMessage.EventName,
                                                  messageType: typeof(TMessage),
                                                  targetHandlerType: targetHandlerType,
                                                  transmissionType: typeof(DataTransmissionModel<TMessage>),
                                                  handlerInvokeDelegate: handlerInvokeDelegate);
    }

    /// <inheritdoc/>
    public bool Equals(WorkflowEventInvokerDescriptor? other)
    {
        return other is not null
               && other.WorkflowName == WorkflowName
               && other.EventName == EventName
               && other.MessageType == MessageType
               && other.TargetHandlerType == TargetHandlerType
               && other.TransmissionType == TransmissionType;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as WorkflowEventInvokerDescriptor);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(WorkflowName, EventName, MessageType, TargetHandlerType, TransmissionType);

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{WorkflowName}].[{EventName}] - Handler: {TargetHandlerType} , Message: {MessageType}, TransmissionType: {TransmissionType}";
    }

    #endregion Public 方法
}
