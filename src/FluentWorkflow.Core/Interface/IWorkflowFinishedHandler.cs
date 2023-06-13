namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程完成处理器
/// </summary>
public interface IWorkflowFinishedHandler<in TWorkflowFinishedMessage>
    : IWorkflowMessageHandler<TWorkflowFinishedMessage>
    where TWorkflowFinishedMessage : IWorkflowFinishedMessage
{
}
