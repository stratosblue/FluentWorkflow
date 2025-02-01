using FluentWorkflow.Abstractions;

namespace FluentWorkflow;

/// <summary>
/// <see cref="IWorkflow"/> 比较器
/// </summary>
public class WorkflowComparer
    : IComparer<IWorkflow>
    , IEqualityComparer<IWorkflow>
    , IComparer<IWorkflowStarter>
    , IEqualityComparer<IWorkflowStarter>
{
    #region Public 属性

    /// <summary>
    /// WorkflowComparer 静态实例
    /// </summary>
    public static WorkflowComparer Instance { get; } = new WorkflowComparer();

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public int Compare(IWorkflow? x, IWorkflow? y) => Equals(x, y) ? 0 : 1;

    /// <inheritdoc/>
    public bool Equals(IWorkflow? x, IWorkflow? y)
    {
        return x is not null
               && y is not null
               && x.Id == y.Id;
    }

    /// <inheritdoc/>
    public int GetHashCode(IWorkflow obj) => obj.Id.GetHashCode();

    #region IComparer<IWorkflowStarter>

    /// <inheritdoc/>
    public int Compare(IWorkflowStarter? x, IWorkflowStarter? y) => Equals(x, y) ? 0 : 1;

    /// <inheritdoc/>
    public bool Equals(IWorkflowStarter? x, IWorkflowStarter? y)
    {
        return x?.Workflow is not null
               && y?.Workflow is not null
               && x.Workflow.Id == y.Workflow.Id;
    }

    /// <inheritdoc/>
    public int GetHashCode(IWorkflowStarter obj) => obj.Workflow.Id.GetHashCode();

    #endregion IComparer<IWorkflowStarter>

    #endregion Public 方法
}
