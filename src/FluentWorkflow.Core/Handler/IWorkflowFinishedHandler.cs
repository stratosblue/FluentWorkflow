using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Handler;

/// <summary>
/// 工作流程完成处理器
/// </summary>
public interface IWorkflowFinishedHandler<in TWorkflowFinishedMessage>
    : IWorkflowMessageHandler<TWorkflowFinishedMessage>
    where TWorkflowFinishedMessage : IWorkflowFinishedMessage
{
}
