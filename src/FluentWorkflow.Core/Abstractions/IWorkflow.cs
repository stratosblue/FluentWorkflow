﻿namespace FluentWorkflow.Abstractions;

/// <summary>
/// 工作流程接口
/// </summary>
public interface IWorkflow
    : IUniqueId
    , ICurrentStage
    , IWorkflowContextCarrier<IWorkflowContext>
{
}
