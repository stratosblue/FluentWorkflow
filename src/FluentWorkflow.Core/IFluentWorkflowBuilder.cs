using FluentWorkflow.Build;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

/// <summary>
/// FluentWorkflow构造器
/// </summary>
public interface IFluentWorkflowBuilder
{
    #region Public 属性

    /// <summary>
    /// 构建上下文属性
    /// </summary>
    public Dictionary<object, object?> Properties { get; }

    /// <inheritdoc cref="IServiceCollection"/>
    public IServiceCollection Services { get; }

    /// <summary>
    /// 构建状态
    /// </summary>
    public WorkflowBuildStateCollection WorkflowBuildStates { get; }

    #endregion Public 属性
}
