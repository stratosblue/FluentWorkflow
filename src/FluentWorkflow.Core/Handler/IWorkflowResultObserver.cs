using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Handler;

/// <summary>
/// 工作流程结果观察器
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
public interface IWorkflowResultObserver<out TWorkflow>
    where TWorkflow : IWorkflow
{
}
