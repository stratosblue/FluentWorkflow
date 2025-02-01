namespace FluentWorkflow.Abstractions;

/// <summary>
/// 工作流程当前阶段
/// </summary>
public interface ICurrentStage
{
    #region Public 属性

    /// <summary>
    /// 工作流程当前阶段
    /// </summary>
    public string Stage { get; }

    #endregion Public 属性
}
