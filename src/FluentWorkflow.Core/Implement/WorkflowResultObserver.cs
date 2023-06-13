using System.ComponentModel;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

/// <summary>
/// 工作流程结果观察器
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
/// <typeparam name="TWorkflowFinishedMessage"></typeparam>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class WorkflowResultObserver<TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary>
    : IWorkflowResultObserver<TWorkflow>
    , IWorkflowFinishedHandler<TWorkflowFinishedMessage>
    where TWorkflow : IWorkflow, TWorkflowBoundary
    where TWorkflowFinishedMessage : IWorkflowFinishedMessage, TWorkflowBoundary
{
    #region Protected 属性

    /// <inheritdoc cref="IServiceProvider"/>
    protected IServiceProvider ServiceProvider { get; }

    #endregion Protected 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowResultObserver{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}"/>
    public WorkflowResultObserver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual async Task HandleAsync(TWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken = default)
    {
        var context = finishedMessage.Context;

        if (context.Flag.HasFlag(WorkflowFlag.HasParentWorkflow | WorkflowFlag.IsBeenAwaited)
            && context.Parent is { } parentContextMetadata
            && ServiceProvider.GetService<IWorkflowContinuatorHub>() is { } workflowContinuatorHub
            && workflowContinuatorHub.TryGet(parentContextMetadata.WorkflowName, parentContextMetadata.Stage, out var workflowContinuator))
        {
            await workflowContinuator.ContinueAsync(finishedMessage, cancellationToken);
        }

        await OnFinishedAsync(finishedMessage, cancellationToken);
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 工作流程结束
    /// </summary>
    /// <param name="finishedMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task OnFinishedAsync(TWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken);

    #endregion Protected 方法
}
