using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

/// <summary>
/// 简单的 <typeparamref name="TWorkflow"/> 构建器
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
/// <typeparam name="TWorkflowContext"></typeparam>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
public abstract class SimpleWorkflowBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWorkflow, TWorkflowContext, TWorkflowBoundary>
    : IWorkflowBuilder<TWorkflow>
    where TWorkflow : IWorkflow, TWorkflowBoundary
    where TWorkflowContext : IWorkflowContext, TWorkflowBoundary
{
    #region Protected 字段

    /// <summary>
    /// <see cref="IServiceProvider"/>
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    #endregion Protected 字段

    #region Public 构造函数

    /// <inheritdoc cref="SimpleWorkflowBuilder{TWorkflow, TWorkflowContext, TWorkflowBoundary}"/>
    public SimpleWorkflowBuilder(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual TWorkflow Build(IEnumerable<KeyValuePair<string, string>> context)
    {
        var typedContext = BuildContext(context) ?? throw new WorkflowInvalidOperationException("上下文不可为空");
        return Build(typedContext);
    }

    /// <inheritdoc/>
    public virtual TWorkflow Build(IWorkflowContext context)
    {
        return context is TWorkflowContext typedContext //如果上下文是对应类型，则不进行复制
               ? Build(typedContext)
               : Build(context.GetSnapshot());
    }

    /// <summary>
    /// 使用已有的上下文实例 <paramref name="context"/> 构建 <typeparamref name="TWorkflow"/>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public virtual TWorkflow Build(TWorkflowContext context)
    {
        var workflow = ActivatorUtilities.CreateInstance<TWorkflow>(ServiceProvider, context);
        return workflow;
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 构建 <typeparamref name="TWorkflowContext"/>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected abstract TWorkflowContext BuildContext(IEnumerable<KeyValuePair<string, string>> context);

    #endregion Protected 方法
}
