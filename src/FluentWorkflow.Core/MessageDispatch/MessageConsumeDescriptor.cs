using System.Collections.Immutable;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 消息消费描述
/// </summary>
/// <param name="EventName">事件名称</param>
/// <param name="InvokerDescriptors">执行器列表</param>
public record class MessageConsumeDescriptor(string EventName, ImmutableArray<WorkflowEventInvokerDescriptor> InvokerDescriptors)
{
    /// <summary>
    /// 单工作流程事件执行程序
    /// </summary>
    public bool SingleWorkflowEventInvoker { get; } = InvokerDescriptors.Length == 1;
}
