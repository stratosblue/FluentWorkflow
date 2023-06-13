using System.ComponentModel;
using FluentWorkflow.Extensions;
using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
/// 工作流延续器
/// </summary>
/// <typeparam name="TWorkflowStageFinalizer"></typeparam>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class WorkflowContinuator<TWorkflowStageFinalizer, TWorkflowBoundary>
    : IWorkflowContinuator
    where TWorkflowStageFinalizer : IWorkflowStageFinalizer, TWorkflowBoundary
{
    #region Protected 字段

    /// <inheritdoc cref="IWorkflowAwaitProcessor"/>
    protected readonly IWorkflowAwaitProcessor WorkflowAwaitProcessor;

    #endregion Protected 字段

    #region Public 构造函数

    /// <inheritdoc/>
    public WorkflowContinuator(IWorkflowAwaitProcessor workflowAwaitProcessor)
    {
        WorkflowAwaitProcessor = workflowAwaitProcessor ?? throw new ArgumentNullException(nameof(workflowAwaitProcessor));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual async Task ContinueAsync(IWorkflowFinishedMessage childWorkflowFinishedMessage, CancellationToken cancellationToken = default)
    {
        var context = childWorkflowFinishedMessage.Context;
        if (context.Parent is not { } parentContextMetadata)
        {
            throw new WorkflowInvalidOperationException($"Context - \"{context.Id}\" has no parent.");
        }

        var finalizer = await GetStageFinalizerAsync(childWorkflowFinishedMessage, parentContextMetadata, cancellationToken);

        var workflowAwaitState = await WorkflowAwaitProcessor.FinishedOneAsync(childWorkflowFinishedMessage, cancellationToken);

        if (!workflowAwaitState.IsFinished)
        {
            return;
        }

        var parentWorkflowContext = workflowAwaitState.ParentWorkflowContext as IWorkflowContext;

        await finalizer.AwaitFinishedAsync(parentWorkflowContext, workflowAwaitState.ChildWorkflowContexts, cancellationToken);

        if (workflowAwaitState.GetFailed(true).FirstOrDefault() is { } failedWorkflowItem
            && failedWorkflowItem.Key is string alias
            && failedWorkflowItem.Value is { } failedWorkflowContext)
        {
            if (!parentWorkflowContext.TryGetFailureMessage(out _)
                && failedWorkflowContext.TryGetFailureMessage(out var failureMessage))
            {
                parentWorkflowContext.SetFailureMessage(failureMessage);
            }

            if (!parentWorkflowContext.TryGetFailureStackTrace(out _)
                && failedWorkflowContext.TryGetFailureStackTrace(out var failureStackTrace))
            {
                var failedWorkflowName = failedWorkflowContext.GetValue(FluentWorkflowConstants.ContextKeys.WorkflowName);
                var failedWorkflowStage = failedWorkflowContext.GetValue(FluentWorkflowConstants.ContextKeys.FailureStage);
                parentWorkflowContext.SetFailureStackTrace($"{failureStackTrace}\n   at {failedWorkflowName} stage {failedWorkflowStage} <- \"{alias}\"[{failedWorkflowContext.Id}]");
            }

            await finalizer.FailAsync(parentWorkflowContext, cancellationToken);
        }
        else
        {
            await finalizer.CompleteAsync(parentWorkflowContext, cancellationToken);
        }
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 获取<see cref="IWorkflowStageFinalizer"/>
    /// </summary>
    /// <param name="childWorkflowFinishedMessage"></param>
    /// <param name="parentContextMetadata"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<TWorkflowStageFinalizer> GetStageFinalizerAsync(IWorkflowFinishedMessage childWorkflowFinishedMessage, WorkflowContextMetadata parentContextMetadata, CancellationToken cancellationToken);

    #endregion Protected 方法
}
