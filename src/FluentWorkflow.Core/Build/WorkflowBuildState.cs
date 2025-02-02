using System.Collections;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Build;

/// <summary>
/// 工作流程构建状态
/// </summary>
public class WorkflowBuildState : IEnumerable<WorkflowEventSubscribeDescriptor>
{
    #region Private 字段

    private readonly Dictionary<string, WorkflowEventSubscribeDescriptor> _workflowEventSubscribeDescriptors = new(StringComparer.OrdinalIgnoreCase);

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public string WorkflowName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowBuildState"/>
    public WorkflowBuildState(string workflowName)
    {
        if (string.IsNullOrWhiteSpace(workflowName))
        {
            throw new ArgumentException($"“{nameof(workflowName)}”不能为 null 或空白。", nameof(workflowName));
        }

        WorkflowName = workflowName;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 添加工作流程事件执行程序描述符
    /// </summary>
    /// <param name="descriptor"></param>
    /// <exception cref="ArgumentException"></exception>
    public void AddEventInvokerDescriptor(WorkflowEventInvokerDescriptor descriptor)
    {
        if (!string.Equals(WorkflowName, descriptor.WorkflowName))
        {
            throw new ArgumentException($"\"{descriptor.WorkflowName}\" not match the \"{WorkflowName}\".");
        }

        if (!_workflowEventSubscribeDescriptors.TryGetValue(descriptor.EventName, out var workflowEventInvokerDescriptors))
        {
            workflowEventInvokerDescriptors = new(descriptor.WorkflowName, descriptor.EventName);
            _workflowEventSubscribeDescriptors.Add(descriptor.EventName, workflowEventInvokerDescriptors);
        }
        workflowEventInvokerDescriptors.AddDescriptor(descriptor);
    }

    /// <inheritdoc/>
    public IEnumerator<WorkflowEventSubscribeDescriptor> GetEnumerator() => _workflowEventSubscribeDescriptors.Values.GetEnumerator();

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{WorkflowName}] - Count: {_workflowEventSubscribeDescriptors.Count}";
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion Public 方法
}
