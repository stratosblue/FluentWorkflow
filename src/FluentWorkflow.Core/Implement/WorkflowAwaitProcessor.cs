using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <inheritdoc cref="IWorkflowAwaitProcessor"/>
public abstract class WorkflowAwaitProcessor : IWorkflowAwaitProcessor
{
    #region Public 方法

    /// <inheritdoc cref="WorkflowAwaitProcessor"/>
    public Task<WorkflowAwaitState> FinishedOneAsync(IWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(finishedMessage);

        var context = finishedMessage.Context;
        if (context.Parent is not { } parentContextSnapshot)
        {
            throw new WorkflowInvalidOperationException($"Context - \"{context.Id}\" has no parent.");
        }

        return OnFinishedOneAsync(parentContextSnapshot, finishedMessage, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task RegisterAsync(IWorkflowContext parentWorkflowContext, IDictionary<string, IWorkflow> childWorkflows, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentWorkflowContext);
        ArgumentNullException.ThrowIfNull(childWorkflows);

        if (!childWorkflows.Any())
        {
            throw new WorkflowInvalidOperationException("Child workflow at least one is required.");
        }

        if (childWorkflows.Any(m => m.Value.IsStarted()))
        {
            throw new WorkflowInvalidOperationException("There has some child workflow already started.");
        }
        return OnRegisterAsync(parentWorkflowContext, childWorkflows, cancellationToken);
    }

    #endregion Public 方法

    #region Protected 方法

    /// <inheritdoc cref="IWorkflowAwaitProcessor.FinishedOneAsync(IWorkflowFinishedMessage, CancellationToken)"/>
    protected abstract Task<WorkflowAwaitState> OnFinishedOneAsync(WorkflowContextSnapshot parentContextSnapshot, IWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken);

    /// <inheritdoc cref="IWorkflowAwaitProcessor.RegisterAsync(IWorkflowContext,IDictionary{string, IWorkflow}, CancellationToken)"/>
    protected abstract Task OnRegisterAsync(IWorkflowContext parentWorkflowContext, IDictionary<string, IWorkflow> childWorkflows, CancellationToken cancellationToken);

    #endregion Protected 方法
}
