namespace FluentWorkflow.Abstractions;

/// <summary>
/// 工作流程名称声明
/// </summary>
public interface IWorkflowNameDeclaration
{
    #region Public 属性

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public abstract static string WorkflowName { get; }

    #endregion Public 属性
}
