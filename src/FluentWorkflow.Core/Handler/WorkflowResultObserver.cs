using System.ComponentModel;
using FluentWorkflow.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Handler;

/// <summary>
/// 工作流程结果观察器
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
/// <typeparam name="TWorkflowFinishedMessage"></typeparam>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class WorkflowResultObserver<TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary>(IServiceProvider serviceProvider)
    : IWorkflowResultObserver<TWorkflow>
    , IWorkflowFinishedHandler<TWorkflowFinishedMessage>
    where TWorkflow : IWorkflow, TWorkflowBoundary
    where TWorkflowFinishedMessage : IWorkflowFinishedMessage, TWorkflowBoundary
{
    #region Protected 属性

    /// <inheritdoc cref="IServiceProvider"/>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    #endregion Protected 属性

    #region Public 方法

    /// <inheritdoc/>
    public virtual async Task HandleAsync(TWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken = default)
    {
        await using var consumptionControlScope = await ServiceProvider.GetWorkingController().ConsumptionControlAsync(TWorkflowFinishedMessage.EventName, finishedMessage, cancellationToken);

        cancellationToken = consumptionControlScope.CancellationToken;

        try
        {
            var context = finishedMessage.Context;

            if (context.Flag.HasFlag(WorkflowFlag.HasParentWorkflow | WorkflowFlag.IsBeenAwaited)
                && context.Parent is { } parentContextSnapshot
                && ServiceProvider.GetService<IWorkflowContinuatorHub>() is { } workflowContinuatorHub
                && workflowContinuatorHub.TryGet(parentContextSnapshot.WorkflowName, parentContextSnapshot.Stage, out var workflowContinuator))
            {
                await workflowContinuator.ContinueAsync(finishedMessage, cancellationToken);
            }

            await OnFinishedAsync(finishedMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            consumptionControlScope.TryThrowWithControlException(ex);
            throw;
        }
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
