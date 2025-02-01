namespace FluentWorkflow.Abstractions;

/// <summary>
/// 工作流程阶段名称声明
/// </summary>
public interface IWorkflowStageNameDeclaration
{
    #region Public 属性

    /// <summary>
    /// 工作流程阶段名称
    /// </summary>
    public abstract static string StageName { get; }

    #endregion Public 属性
}
