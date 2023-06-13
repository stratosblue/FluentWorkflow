namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程阶段消息
/// </summary>
public interface IWorkflowStageMessage
    : IWorkflowMessage
    , ICurrentStage
    , IWorkflowStage
{
}
