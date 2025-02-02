using FluentWorkflow.Abstractions;

namespace FluentWorkflow.RabbitMQ;

internal record class ConsumeDescriptor(string EventName, WorkflowEventInvokerDescriptor[] InvokerDescriptors, MessageRequeuePolicy RequeuePolicy, TimeSpan RequeueDelay)
{
    /// <summary>
    /// 单工作流程事件执行程序
    /// </summary>
    public bool SingleWorkflowEventInvoker { get; } = InvokerDescriptors.Length == 1;
}
