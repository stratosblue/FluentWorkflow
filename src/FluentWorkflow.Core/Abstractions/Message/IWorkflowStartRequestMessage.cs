namespace FluentWorkflow.Abstractions;

/// <summary>
/// 工作流程启动请求消息
/// </summary>
public interface IWorkflowStartRequestMessage
    : IWorkflowMessage
    , IWorkflowContextCarrier<IWorkflowContext>
    , IWorkflowNameDeclaration
    , IEventNameDeclaration
{
}
