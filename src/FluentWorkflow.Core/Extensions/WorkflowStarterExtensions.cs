using System.ComponentModel;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WorkflowStarterExtensions
{
    #region Public 方法

    /// <inheritdoc cref="IWorkflowStarter.StartAsync(CancellationToken)"/>
    public static Task StartAsync(this IWorkflowStarter workflowStarter, CancellationToken cancellationToken = default)
    {
        return workflowStarter.StartAsync(cancellationToken);
    }

    #endregion Public 方法
}
