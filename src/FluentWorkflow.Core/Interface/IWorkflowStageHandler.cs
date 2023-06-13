namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程阶段处理器
/// </summary>
/// <typeparam name="TStageMessage">处理的阶段消息</typeparam>
public interface IWorkflowStageHandler<in TStageMessage>
    : IWorkflowMessageHandler<TStageMessage>
    where TStageMessage : IWorkflowStageMessage, IEventNameDeclaration
{
}
