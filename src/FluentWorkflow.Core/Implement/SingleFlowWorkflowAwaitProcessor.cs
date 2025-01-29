using FluentWorkflow.Extensions;
using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
/// 单流程 <inheritdoc cref="IWorkflowAwaitProcessor"/>
/// </summary>
internal sealed class SingleflowWorkflowAwaitProcessor : WorkflowAwaitProcessor
{
    #region Protected 方法

    /// <inheritdoc/>
    protected override Task<WorkflowAwaitState> OnFinishedOneAsync(WorkflowContextSnapshot parentContextSnapshot, IWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken)
    {
        var context = finishedMessage.Context;

        if (!context.Flag.HasFlag(WorkflowFlag.UniqueChildWorkflow))
        {
            throw new WorkflowInvalidOperationException($"The context \"{context.Id}\" not the unique child workflow.");
        }

        var alias = context.TryGetChildWorkflowAlias(out var aliasValue) ? aliasValue : string.Empty;

        var childWorkflowContexts = new Dictionary<string, IWorkflowContext?>(1)
        {
            { alias, context }
        };

        var awaitState = new WorkflowAwaitState(parentContextSnapshot, true, childWorkflowContexts);
        return Task.FromResult(awaitState);
    }

    /// <inheritdoc/>
    protected override Task OnRegisterAsync(IWorkflowContext parentWorkflowContext, IDictionary<string, IWorkflow> childWorkflows, CancellationToken cancellationToken)
    {
        if (childWorkflows.Count != 1)
        {
            throw new WorkflowInvalidOperationException("Default workflow await processor does not support multi child workflow.");
        }

        return Task.CompletedTask;
    }

    #endregion Protected 方法
}
