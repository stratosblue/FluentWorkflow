namespace FluentWorkflow;

/// <summary>
/// 标记工作流程阶段特性
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public class WorkflowStageAttribute : Attribute
{
    #region Public 属性

    /// <summary>
    /// 阶段
    /// </summary>
    public string Stage { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="WorkflowStageAttribute"/>
    /// </summary>
    /// <param name="stage">阶段</param>
    /// <exception cref="ArgumentException"></exception>
    public WorkflowStageAttribute(string stage)
    {
        WorkflowException.ThrowIfNullOrWhiteSpace(stage);

        Stage = stage;
    }

    #endregion Public 构造函数
}
