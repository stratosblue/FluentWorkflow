using System.ComponentModel;
using System.Diagnostics;
using FluentWorkflow.Extensions;
using FluentWorkflow.Interface;
using Microsoft.Extensions.Logging;

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

    /// <inheritdoc cref="ILogger"/>
    protected readonly ILogger Logger;

    /// <inheritdoc cref="IServiceProvider"/>
    protected readonly IServiceProvider ServiceProvider;

    /// <inheritdoc cref="IWorkflowAwaitProcessor"/>
    protected readonly IWorkflowAwaitProcessor WorkflowAwaitProcessor;

    #endregion Protected 字段

    #region Public 构造函数

    /// <inheritdoc/>
    public WorkflowContinuator(IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
    {
        WorkflowAwaitProcessor = workflowAwaitProcessor ?? throw new ArgumentNullException(nameof(workflowAwaitProcessor));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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

        var workflowAwaitState = await WorkflowAwaitProcessor.FinishedOneAsync(childWorkflowFinishedMessage, cancellationToken);

        if (!workflowAwaitState.IsFinished) //未完成等待，直接返回，等待下一个子流程完成后再次进行检查
        {
            return;
        }

        var finalizer = await GetStageFinalizerAsync(childWorkflowFinishedMessage, parentContextMetadata, cancellationToken);

        var parentWorkflowContext = workflowAwaitState.ParentWorkflowContext as IWorkflowContext;

        try
        {
            await finalizer.AwaitFinishedAsync(parentWorkflowContext, workflowAwaitState.ChildWorkflowContexts, cancellationToken);
        }
        catch (Exception exception)
        {
            var currentAlias = childWorkflowFinishedMessage.Context.TryGetChildWorkflowAlias(out var aliasValue) ? aliasValue : "Unknown";
            Logger.LogError(exception, "Await finished child workflow \"{Alias}\" failed.", currentAlias);
            parentWorkflowContext.SetFailureMessage($"Await finished child workflow \"{currentAlias}\" failed: {exception.Message}");
            parentWorkflowContext.SetFailureStackTrace(exception.StackTrace ?? new StackTrace(1, fNeedFileInfo: true).ToString());

            //执行等待失败，直接失败
            await finalizer.FailAsync(parentWorkflowContext, cancellationToken);
            return;
        }

        if (workflowAwaitState.GetFailed(true).FirstOrDefault() is { } failedWorkflowItem
            && failedWorkflowItem.Key is string alias
            && failedWorkflowItem.Value is { } failedWorkflowContext)   //获取第一个失败子流程
        {
            if (!parentWorkflowContext.TryGetFailureMessage(out _)
                && failedWorkflowContext.TryGetFailureMessage(out var failureMessage))  //如果父流程上下文当前没有失败消息，则设置为第一个失败子流程消息
            {
                parentWorkflowContext.SetFailureMessage(failureMessage);
            }

            if (!parentWorkflowContext.TryGetFailureStackTrace(out _)
                && failedWorkflowContext.TryGetFailureStackTrace(out var failureStackTrace))    //如果父流程上下文当前没有失败堆栈，则设置为第一个失败子流程堆栈
            {
                var failedWorkflowName = failedWorkflowContext.GetValue<string>(FluentWorkflowConstants.ContextKeys.WorkflowName);
                var failedWorkflowStage = failedWorkflowContext.GetValue<string>(FluentWorkflowConstants.ContextKeys.FailureStage);
                parentWorkflowContext.SetFailureStackTrace($"{failureStackTrace}\n   at {failedWorkflowName} stage {failedWorkflowStage} <- \"{alias}\"[{failedWorkflowContext.Id}]");
            }

            await finalizer.FailAsync(parentWorkflowContext, cancellationToken);
        }
        else    //没有失败子流程，完成阶段
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
