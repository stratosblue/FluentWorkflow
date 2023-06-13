using System.ComponentModel;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

/// <summary>
/// <inheritdoc cref="IWorkflowStateMachineDriver{TWorkflow, TStageCompletedMessage, TFailureMessage}"/>
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
/// <typeparam name="TWorkflowContext"></typeparam>
/// <typeparam name="TWorkflowStateMachine"></typeparam>
/// <typeparam name="TStageCompletedMessage"></typeparam>
/// <typeparam name="TFailureMessage"></typeparam>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class WorkflowStateMachineDriver<TWorkflow, TWorkflowContext, TWorkflowStateMachine, TStageCompletedMessage, TFailureMessage, TWorkflowBoundary>
    : IWorkflowStateMachineDriver<TWorkflow, TStageCompletedMessage, TFailureMessage>
    where TWorkflow : IWorkflow, TWorkflowBoundary
    where TWorkflowContext : IWorkflowContext, TWorkflowBoundary
    where TWorkflowStateMachine : IWorkflowStateMachine, TWorkflowBoundary
    where TStageCompletedMessage : IWorkflowStageCompletedMessage, IWorkflowContextCarrier<TWorkflowContext>, TWorkflowBoundary
    where TFailureMessage : IWorkflowFailureMessage, IWorkflowContextCarrier<TWorkflowContext>, TWorkflowBoundary
{
    #region Protected 字段

    /// <inheritdoc cref="IWorkflowMessageDispatcher"/>
    protected readonly IWorkflowMessageDispatcher MessageDispatcher;

    /// <inheritdoc cref="IServiceProvider"/>
    protected readonly IServiceProvider ServiceProvider;

    /// <inheritdoc cref="IWorkflowBuilder{TWorkflow}"/>
    protected readonly IWorkflowBuilder<TWorkflow> WorkflowBuilder;

    #endregion Protected 字段

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowStateMachineDriver{TWorkflow, TWorkflowContext, TWorkflowStateMachine, TStageCompletedMessage, TFailureMessage, TWorkflowBoundary}"/>
    public WorkflowStateMachineDriver(IWorkflowBuilder<TWorkflow> workflowBuilder, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
    {
        WorkflowBuilder = workflowBuilder ?? throw new ArgumentNullException(nameof(workflowBuilder));
        MessageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual Task HandleAsync(TStageCompletedMessage message, CancellationToken cancellationToken)
    {
        var activity = ActivitySource.StartActivity($"{DiagnosticConstants.ActivityNames.StageMoving} - {message.Stage}", System.Diagnostics.ActivityKind.Consumer);
        var task = DoInputAsync(message, cancellationToken);
        if (activity != null)
        {
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.Message, PrettyJSONObject.Create(message));
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.StageState, "completed");
            task.ContinueWith(static (_, state) => ((IDisposable)state!).Dispose(), activity, CancellationToken.None);
        }
        return task;
    }

    /// <inheritdoc/>
    public virtual Task HandleAsync(TFailureMessage message, CancellationToken cancellationToken)
    {
        var activity = ActivitySource.StartActivity($"{DiagnosticConstants.ActivityNames.StageMoving} - {message.Stage}", System.Diagnostics.ActivityKind.Consumer);
        var task = DoInputAsync(message, cancellationToken);
        if (activity != null)
        {
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.Message, PrettyJSONObject.Create(message));
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.StageState, "failure");
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.FailureMessage, message.Message);
            task.ContinueWith(static (_, state) => ((IDisposable)state!).Dispose(), activity, CancellationToken.None);
        }
        return task;
    }

    #endregion Public 方法

    #region Protected 方法

    /// <inheritdoc cref="HandleAsync(TStageCompletedMessage, CancellationToken)"/>
    protected abstract Task DoInputAsync(TStageCompletedMessage message, CancellationToken cancellationToken);

    /// <inheritdoc cref="HandleAsync(TFailureMessage, CancellationToken)"/>
    protected abstract Task DoInputAsync(TFailureMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// 驱动状态机
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task InternalDriveAsync(TWorkflowStateMachine stateMachine, CancellationToken cancellationToken)
    {
        return stateMachine.MoveNextAsync(cancellationToken);
    }

    /// <summary>
    /// 恢复状态机
    /// </summary>
    /// <param name="workflowContextCarrier"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task<TWorkflowStateMachine> RestoreStateMachineAsync(IWorkflowContextCarrier<TWorkflowContext> workflowContextCarrier, CancellationToken cancellationToken)
    {
        var workflow = WorkflowBuilder.Build(workflowContextCarrier.Context);
        var stateMachine = ActivatorUtilities.CreateInstance<TWorkflowStateMachine>(ServiceProvider, workflow, MessageDispatcher);
        return Task.FromResult(stateMachine);
    }

    #endregion Protected 方法
}
