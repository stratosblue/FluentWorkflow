using System.ComponentModel;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Interface;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

/// <summary>
/// 工作流程调度器
/// </summary>
/// <typeparam name="TWorkflow">工作流程类型</typeparam>
/// <typeparam name="TWorkflowStateMachine">工作流程状态机类型</typeparam>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class WorkflowScheduler<TWorkflow, TWorkflowStateMachine, TWorkflowBoundary>
    : IWorkflowScheduler<TWorkflow>
    where TWorkflow : IWorkflow, IWorkflowNameDeclaration, TWorkflowBoundary
    where TWorkflowStateMachine : IWorkflowStateMachine, TWorkflowBoundary
{
    #region Private 字段

    private readonly IWorkflowDiagnosticSource _diagnosticSource;

    #endregion Private 字段

    #region Protected 字段

    /// <inheritdoc cref="IWorkflowMessageDispatcher"/>
    protected readonly IWorkflowMessageDispatcher MessageDispatcher;

    /// <inheritdoc cref="IObjectSerializer"/>
    protected readonly IObjectSerializer ObjectSerializer;

    /// <inheritdoc cref="IServiceProvider"/>
    protected readonly IServiceProvider ServiceProvider;

    #endregion Protected 字段

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowScheduler{TWorkflow, TWorkflowStateMachine, TWorkflowBoundary}"/>
    public WorkflowScheduler(IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
    {
        MessageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        ObjectSerializer = serviceProvider.GetRequiredService<IObjectSerializer>();
        _diagnosticSource = serviceProvider.GetRequiredService<IWorkflowDiagnosticSource>();
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual Task StartAsync(TWorkflow workflow, CancellationToken cancellationToken = default)
    {
        var activity = ActivitySource.StartActivity($"{DiagnosticConstants.ActivityNames.WorkflowStarting} - {TWorkflow.WorkflowName}", System.Diagnostics.ActivityKind.Consumer);

        _diagnosticSource.WorkflowScheduleStart(workflow);

        var stateMachine = CreateStateMachine(workflow);
        var task = stateMachine.MoveNextAsync(cancellationToken);

        if (activity != null)
        {
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.Context, PrettyJSONObject.Create(workflow.Context, ObjectSerializer));
            task.DisposeActivityWhenTaskCompleted(activity);
        }

        return task;
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 创建状态机
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>
    protected abstract TWorkflowStateMachine CreateStateMachine(TWorkflow workflow);

    #endregion Protected 方法
}
