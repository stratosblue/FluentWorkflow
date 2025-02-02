using System.Collections;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Build;

/// <summary>
/// 工作流程事件订阅描述符
/// </summary>
public class WorkflowEventSubscribeDescriptor : IEnumerable<WorkflowEventInvokerDescriptor>
{
    #region Private 字段

    /// <summary>
    /// 执行程序描述符集合
    /// </summary>
    private readonly HashSet<WorkflowEventInvokerDescriptor> _invokerDescriptors = new();

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 事件名称
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public string WorkflowName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowEventSubscribeDescriptor"/>
    public WorkflowEventSubscribeDescriptor(string workflowName, string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        WorkflowName = workflowName;
        EventName = eventName;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 添加工作流程事件执行程序描述符
    /// </summary>
    /// <param name="descriptor"></param>
    /// <exception cref="ArgumentException"></exception>
    public void AddDescriptor(WorkflowEventInvokerDescriptor descriptor)
    {
        if (!string.Equals(WorkflowName, descriptor.WorkflowName)
            || !string.Equals(EventName, descriptor.EventName))
        {
            throw new ArgumentException($"\"{descriptor.WorkflowName}\"-\"{descriptor.EventName}\" not match the \"{WorkflowName}\"-\"{EventName}\".");
        }
        _invokerDescriptors.Add(descriptor);
    }

    /// <inheritdoc/>
    public IEnumerator<WorkflowEventInvokerDescriptor> GetEnumerator() => _invokerDescriptors.GetEnumerator();

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{WorkflowName}].[{EventName}] - Count: {_invokerDescriptors.Count}";
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion Public 方法
}
