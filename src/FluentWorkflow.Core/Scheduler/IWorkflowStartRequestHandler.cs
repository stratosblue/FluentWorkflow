using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Scheduler;

/// <summary>
/// 工作流程启动请求处理器
/// </summary>
/// <typeparam name="TStartRequestMessage">工作流程启动请求消息</typeparam>
public interface IWorkflowStartRequestHandler<in TStartRequestMessage>
    : IWorkflowMessageHandler<TStartRequestMessage>
    where TStartRequestMessage : IWorkflowStartRequestMessage
{
}
