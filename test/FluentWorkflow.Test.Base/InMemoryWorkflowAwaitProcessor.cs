using System.Collections.Concurrent;
using FluentWorkflow.Interface;

namespace FluentWorkflow;

public class InMemoryWorkflowAwaitProcessor : WorkflowAwaitProcessor
{
    #region Private 类

    private class ContextWithChildWorkflowState
    {
        #region Public 属性

        public Dictionary<string, IEnumerable<KeyValuePair<string, string>>?> Children { get; set; } = new();

        public IEnumerable<KeyValuePair<string, string>>? ParentContext { get; set; }

        #endregion Public 属性
    }

    #endregion Private 类

    #region Private 字段

    private readonly ConcurrentDictionary<string, ContextWithChildWorkflowState> _store = new();

    #endregion Private 字段

    #region Protected 方法

    protected override Task<WorkflowAwaitState> OnFinishedOneAsync(WorkflowContextSnapshot parentContextSnapshot, IWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken)
    {
        if (!_store.TryGetValue(parentContextSnapshot.Id, out var state))
        {
            throw new WorkflowInvalidOperationException($"No state for {parentContextSnapshot.Id}");
        }
        lock (state)
        {
            var alias = finishedMessage.Context.State.Alias ?? throw new InvalidOperationException("The context has not alias.");
            state.Children[alias] = finishedMessage.Context.GetSnapshot();

            var childWorkflowContexts = state.Children.ToDictionary(m => m.Key,
                                                                    m => m.Value is null ? null : (IWorkflowContext)new WorkflowContextSnapshot(m.Value));

            if (!state.Children.Any(m => m.Value is null))
            {
                return Task.FromResult(new WorkflowAwaitState(parentContextSnapshot, true, childWorkflowContexts));
            }
            return Task.FromResult(new WorkflowAwaitState(parentContextSnapshot, false, childWorkflowContexts));
        }
    }

    protected override Task OnRegisterAsync(IWorkflowContext parentWorkflowContext, IDictionary<string, IWorkflow> childWorkflows, CancellationToken cancellationToken)
    {
        var state = _store.GetOrAdd(parentWorkflowContext.Id, _ => new());
        lock (state)
        {
            state.ParentContext = parentWorkflowContext.GetSnapshot();
            foreach (var (alias, item) in childWorkflows)
            {
                state.Children.Add(alias, null);
            }
        }
        return Task.CompletedTask;
    }

    #endregion Protected 方法
}
