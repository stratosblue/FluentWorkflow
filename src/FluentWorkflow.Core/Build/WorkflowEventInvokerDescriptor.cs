namespace FluentWorkflow.Build;

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
    /// 目标类型
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public string WorkflowName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowEventInvokerDescriptor"/>
    public WorkflowEventInvokerDescriptor(string workflowName, string eventName, Type messageType, Type targetType, EventMessageHandlerInvokeDelegate handlerInvokeDelegate)
    {
        if (string.IsNullOrWhiteSpace(workflowName))
        {
            throw new ArgumentException($"“{nameof(workflowName)}”不能为 null 或空白。", nameof(workflowName));
        }

        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException($"“{nameof(eventName)}”不能为 null 或空。", nameof(eventName));
        }

        WorkflowName = workflowName;
        EventName = eventName;
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        HandlerInvokeDelegate = handlerInvokeDelegate ?? throw new ArgumentNullException(nameof(handlerInvokeDelegate));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public bool Equals(WorkflowEventInvokerDescriptor? other)
    {
        return other is not null
               && other.WorkflowName == WorkflowName
               && other.EventName == EventName
               && other.MessageType == MessageType
               && other.TargetType == TargetType;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as WorkflowEventInvokerDescriptor);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(WorkflowName, EventName, MessageType, TargetType);

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{WorkflowName}].[{EventName}] - Handler: {TargetType} , Message: {MessageType}";
    }

    #endregion Public 方法
}
