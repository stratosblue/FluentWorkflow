using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.MessageDispatch;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Scheduler;

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
public abstract class WorkflowStateMachineDriver<TWorkflow, TWorkflowContext, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWorkflowStateMachine, TStageCompletedMessage, TFailureMessage, TWorkflowBoundary>
    : IWorkflowStateMachineDriver<TWorkflow, TStageCompletedMessage, TFailureMessage>
    where TWorkflow : IWorkflow, TWorkflowBoundary
    where TWorkflowContext : IWorkflowContext, TWorkflowBoundary
    where TWorkflowStateMachine : IWorkflowStateMachine, TWorkflowBoundary
    where TStageCompletedMessage : IWorkflowStageCompletedMessage, IEventName, IWorkflowContextCarrier<TWorkflowContext>, TWorkflowBoundary
    where TFailureMessage : IWorkflowFailureMessage, IWorkflowContextCarrier<TWorkflowContext>, TWorkflowBoundary
{
    #region Protected 字段

    /// <inheritdoc cref="IWorkflowMessageDispatcher"/>
    protected readonly IWorkflowMessageDispatcher MessageDispatcher;

    /// <inheritdoc cref="IObjectSerializer"/>
    protected readonly IObjectSerializer ObjectSerializer;

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

        ObjectSerializer = serviceProvider.GetRequiredService<IObjectSerializer>();
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual async Task HandleAsync(TStageCompletedMessage message, CancellationToken cancellationToken)
    {
        if (Activity.Current is { } activity)
        {
            activity.AddEvent($"{DiagnosticConstants.ActivityNames.StageMoving} - {message.Stage}");
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.Message, PrettyJSONObject.Create(message, ObjectSerializer));
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.StageState, "completed");
        }

        await using var consumptionControlScope = await ServiceProvider.GetWorkingController().ConsumptionControlAsync(message.EventName, message, cancellationToken);

        cancellationToken = consumptionControlScope.CancellationToken;

        try
        {
            await DoInputAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            consumptionControlScope.TryThrowWithControlException(ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task HandleAsync(TFailureMessage message, CancellationToken cancellationToken)
    {
        if (Activity.Current is { } activity)
        {
            activity.AddEvent($"{DiagnosticConstants.ActivityNames.StageMoving} - {message.Stage}");
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.Message, PrettyJSONObject.Create(message, ObjectSerializer));
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.StageState, "failure");
            activity.AddTag(DiagnosticConstants.ActivityNames.TagKeys.FailureMessage, message.Message);
        }

        await using var consumptionControlScope = await ServiceProvider.GetWorkingController().ConsumptionControlAsync(TFailureMessage.EventName, message, cancellationToken);

        cancellationToken = consumptionControlScope.CancellationToken;

        try
        {
            await DoInputAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            consumptionControlScope.TryThrowWithControlException(ex);
            throw;
        }
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
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task<TWorkflowStateMachine> RestoreStateMachineAsync(IWorkflowContext context, CancellationToken cancellationToken)
    {
        ThrowIfContextInvalid(context);

        var workflow = WorkflowBuilder.Build(context);
        var stateMachine = ActivatorUtilities.CreateInstance<TWorkflowStateMachine>(ServiceProvider, workflow, MessageDispatcher);
        return Task.FromResult(stateMachine);
    }

    /// <summary>
    /// 如果上下文不正确则抛出异常
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="WorkflowInvalidOperationException"></exception>
    protected void ThrowIfContextInvalid(IWorkflowContext context)
    {
        if (!ValidationContext(context))
        {
            throw new WorkflowInvalidOperationException($"The context stage \"{context.Stage}\" not belong to \"{typeof(TWorkflow)}\".");
        }
    }

    /// <summary>
    /// 检查上下文是否属于当前工作流程 <typeparamref name="TWorkflow"/>
    /// </summary>
    /// <param name="context"></param>
    protected abstract bool ValidationContext(IWorkflowContext context);

    #endregion Protected 方法
}
