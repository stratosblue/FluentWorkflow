namespace FluentWorkflow;

/// <summary>
/// 标记工作流程名称特性
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public class WorkflowNameAttribute : Attribute
{
    #region Public 属性

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="WorkflowNameAttribute"/>
    /// </summary>
    /// <param name="name">名称</param>
    /// <exception cref="ArgumentException"></exception>
    public WorkflowNameAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    #endregion Public 构造函数
}
