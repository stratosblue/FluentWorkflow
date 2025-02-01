using System.Collections;
using System.Diagnostics;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Build;

/// <summary>
/// 工作流程构建状态集合
/// </summary>
public class WorkflowBuildStateCollection : IEnumerable<WorkflowBuildState>
{
    #region Private 字段

    private readonly Dictionary<string, WorkflowBuildState> _workflowBuildStates = new(StringComparer.OrdinalIgnoreCase);

    #endregion Private 字段

    #region Public 方法

    /// <summary>
    /// 添加工作流程事件执行程序描述符
    /// </summary>
    /// <param name="descriptor"></param>
    /// <exception cref="ArgumentException"></exception>
    public void AddEventInvokerDescriptor(WorkflowEventInvokerDescriptor descriptor)
    {
        if (!_workflowBuildStates.TryGetValue(descriptor.WorkflowName, out var workflowBuildState))
        {
            workflowBuildState = new(descriptor.WorkflowName);
            _workflowBuildStates.Add(descriptor.WorkflowName, workflowBuildState);
        }

        workflowBuildState.AddEventInvokerDescriptor(descriptor);
    }

    /// <summary>
    /// 添加工作流程事件执行程序描述符
    /// </summary>
    public void AddEventInvokerDescriptor<TWorkflow, THandler, TMessage, TWorkflowBoundary>()
        where TWorkflow : IWorkflowNameDeclaration, TWorkflowBoundary
        where THandler : IWorkflowMessageHandler<TMessage>, TWorkflowBoundary
        where TMessage : IWorkflowMessage, IEventNameDeclaration, TWorkflowBoundary
    {
        var descriptor = new WorkflowEventInvokerDescriptor(TWorkflow.WorkflowName, TMessage.EventName, typeof(TMessage), typeof(THandler), InvokeHandler);

        AddEventInvokerDescriptor(descriptor);

        [DebuggerStepThrough]
        [StackTraceHidden]
        static Task InvokeHandler(object instance, object message, CancellationToken cancellationToken)
        {
            return ((THandler)instance).HandleAsync((TMessage)message, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<WorkflowBuildState> GetEnumerator() => _workflowBuildStates.Values.GetEnumerator();

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Count: {_workflowBuildStates.Count}";
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion Public 方法
}
