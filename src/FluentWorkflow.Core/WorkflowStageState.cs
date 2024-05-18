using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
/// 工作流程阶段状态<br/>
/// 当内部用于恢复流程时，不同状态处理方式不同：<br/>
/// <see cref="Created"/>：使用当前阶段对应的 <see cref="IWorkflowStageMessage"/> 再次执行状态机<br/>
/// <see cref="Scheduled"/>：分发消息<br/>
/// <see cref="Finished"/>：使用当前阶段对应的 <see cref="IWorkflowStageCompletedMessage"/> 再次调用阶段完成方法<br/>
/// </summary>
public enum WorkflowStageState : byte
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 已创建
    /// </summary>
    Created = 1,

    /// <summary>
    /// 已计划
    /// </summary>
    Scheduled = 50,

    /// <summary>
    /// 已结束
    /// </summary>
    Finished = 200,
}
