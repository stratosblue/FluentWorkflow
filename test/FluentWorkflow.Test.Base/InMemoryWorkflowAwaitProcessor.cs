﻿using System.Collections.Concurrent;
using FluentWorkflow.Extensions;
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

    protected override Task<WorkflowAwaitState> OnFinishedOneAsync(WorkflowContextMetadata parentContextMetadata, IWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken)
    {
        if (!_store.TryGetValue(parentContextMetadata.Id, out var state))
        {
            throw new WorkflowInvalidOperationException($"No state for {parentContextMetadata.Id}");
        }
        lock (state)
        {
            state.Children[finishedMessage.Context.GetChildWorkflowAlias()] = finishedMessage.Context.GetSnapshot();

            var childWorkflowContexts = state.Children.ToDictionary(m => m.Key,
                                                                    m => m.Value is null ? null : (IWorkflowContext)new WorkflowContextMetadata(m.Value));

            if (!state.Children.Any(m => m.Value is null))
            {
                return Task.FromResult(new WorkflowAwaitState(parentContextMetadata, true, childWorkflowContexts));
            }
            return Task.FromResult(new WorkflowAwaitState(parentContextMetadata, false, childWorkflowContexts));
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
