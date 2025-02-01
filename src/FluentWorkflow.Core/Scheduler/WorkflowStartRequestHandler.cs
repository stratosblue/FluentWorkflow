using System.ComponentModel;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Scheduler;

/// <summary>
/// 工作流程启动请求处理器
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
/// <typeparam name="TWorkflowContext"></typeparam>
/// <typeparam name="TStartRequestMessage"></typeparam>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class WorkflowStartRequestHandler<TWorkflow, TWorkflowContext, TStartRequestMessage, TWorkflowBoundary>
    : IWorkflowStartRequestHandler<TStartRequestMessage>
    where TWorkflowContext : WorkflowContext, TWorkflowBoundary
    where TWorkflow : IWorkflow, IWorkflowContextCarrier<TWorkflowContext>, TWorkflowBoundary
    where TStartRequestMessage : IWorkflowStartRequestMessage, TWorkflowBoundary
{
    #region Private 字段

    private readonly IWorkflowBuilder<TWorkflow> _workflowBuilder;

    private readonly IWorkflowScheduler<TWorkflow> _workflowScheduler;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowStartRequestHandler{TWorkflow, TWorkflowContext, TStartRequestMessage, TWorkflowBoundary}"/>
    public WorkflowStartRequestHandler(IWorkflowBuilder<TWorkflow> workflowBuilder, IWorkflowScheduler<TWorkflow> workflowScheduler)
    {
        _workflowBuilder = workflowBuilder ?? throw new ArgumentNullException(nameof(workflowBuilder));
        _workflowScheduler = workflowScheduler ?? throw new ArgumentNullException(nameof(workflowScheduler));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual Task HandleAsync(TStartRequestMessage requestMessage, CancellationToken cancellationToken)
    {
        var workflow = _workflowBuilder.Build(requestMessage.Context);

        return _workflowScheduler.StartAsync(workflow, cancellationToken);
    }

    #endregion Public 方法
}
