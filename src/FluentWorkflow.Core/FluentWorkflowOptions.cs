namespace FluentWorkflow;

/// <summary>
/// 工作流程配置
/// </summary>
public sealed class FluentWorkflowOptions
{
    #region Public 属性

    /// <summary>
    /// 工作流程延续器集合
    /// </summary>
    public WorkflowContinuatorCollection Continuators { get; } = new();

    #endregion Public 属性
}
