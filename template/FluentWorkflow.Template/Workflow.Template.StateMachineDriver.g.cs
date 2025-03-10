﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Extensions;
using FluentWorkflow.MessageDispatch;
using FluentWorkflow.Scheduler;
using TemplateNamespace.Template.Message;

namespace TemplateNamespace.Template.Internal;

/// <summary>
/// <see cref="TemplateWorkflow"/> 的状态机驱动器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateWorkflowStateMachineDriverBase
    : WorkflowStateMachineDriver<TemplateWorkflow, TemplateWorkflowContext, TemplateWorkflowStateMachine, TemplateStageCompletedMessageBase, TemplateFailureMessage, ITemplateWorkflow>
    , IWorkflowMessageHandler<TemplateStageCompletedMessageBase>
    , IWorkflowMessageHandler<TemplateFailureMessage>
    , IWorkflowResumer<TemplateWorkflow>
    , ITemplateWorkflow
{
    /// <inheritdoc cref="TemplateWorkflowStateMachineDriverBase"/>
    protected TemplateWorkflowStateMachineDriverBase(IWorkflowBuilder<TemplateWorkflow> workflowBuilder, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
        : base(workflowBuilder, messageDispatcher, serviceProvider)
    {
    }

    /// <inheritdoc/>
    public virtual async Task ResumeAsync(IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context is not TemplateWorkflowContext { } typedContext)
        {
            throw new WorkflowInvalidOperationException($"The context must be a instance of \"{typeof(TemplateWorkflowContext)}\".");
        }

        ThrowIfContextInvalid(context);

        var state = context.State.StageState;
        if (state == WorkflowStageState.Unknown)
        {
            throw new WorkflowInvalidOperationException("The stage state is invalid. Can not resume.");
        }

        switch (state)
        {
            case WorkflowStageState.Created:
                {
                    var stateMachine = await RestoreStateMachineAsync(context, cancellationToken);
                    await stateMachine.MoveNextAsync(cancellationToken);
                    break;
                }

            case WorkflowStageState.Scheduled:
                {
                    var currentStage = context.Stage;
                    switch (currentStage)
                    {
                        case TemplateStages.Stage1CAUK:
                            {
                                var stageMessage = new StageStage1CAUKMessage(WorkflowMessageIdProvider.Generate(), typedContext);
                                await MessageDispatcher.PublishAsync(stageMessage, cancellationToken);
                                break;
                            }
                        case TemplateStages.Stage2BPTG:
                            {
                                var stageMessage = new StageStage2BPTGMessage(WorkflowMessageIdProvider.Generate(), typedContext);
                                await MessageDispatcher.PublishAsync(stageMessage, cancellationToken);
                                break;
                            }
                        case TemplateStages.Stage3AWBN:
                            {
                                var stageMessage = new StageStage3AWBNMessage(WorkflowMessageIdProvider.Generate(), typedContext);
                                await MessageDispatcher.PublishAsync(stageMessage, cancellationToken);
                                break;
                            }
                        default:
                            throw new WorkflowInvalidOperationException($"Unsupported scheduled stage：{currentStage}");
                    }
                    break;
                }

            case WorkflowStageState.Finished:
                {
                    var currentStage = context.Stage;
                    TemplateStageCompletedMessageBase stageCompletedMessage = currentStage switch
                    {
                        TemplateStages.Stage1CAUK => new StageStage1CAUKCompletedMessage(WorkflowMessageIdProvider.Generate(), typedContext),
                        TemplateStages.Stage2BPTG => new StageStage2BPTGCompletedMessage(WorkflowMessageIdProvider.Generate(), typedContext),
                        TemplateStages.Stage3AWBN => new StageStage3AWBNCompletedMessage(WorkflowMessageIdProvider.Generate(), typedContext),
                        _ => throw new WorkflowInvalidOperationException($"Unsupported finished stage：{currentStage}"),
                    };

                    await DoInputAsync(stageCompletedMessage, cancellationToken);
                    break;
                }

            default:
                throw new WorkflowInvalidOperationException($"Unsupported stage state \"{state}\".");
        }
    }

    /// <inheritdoc/>
    protected override async Task DoInputAsync(TemplateStageCompletedMessageBase message, CancellationToken cancellationToken)
    {
        var stateMachine = await RestoreStateMachineAsync(message.Context, cancellationToken);

        if (await stateMachine.SetStageCompletedAsync(message, cancellationToken))
        {
            await InternalDriveAsync(stateMachine, cancellationToken);
        }
    }

    /// <inheritdoc/>
    protected override async Task DoInputAsync(TemplateFailureMessage message, CancellationToken cancellationToken)
    {
        var stateMachine = await RestoreStateMachineAsync(message.Context, cancellationToken);

        if (await stateMachine.SetFailedAsync(message, cancellationToken))
        {
            await InternalDriveAsync(stateMachine, cancellationToken);
        }
    }

    /// <inheritdoc/>
    protected override bool ValidationContext(IWorkflowContext context) => TemplateStages.StageIds.Contains(context.Stage);
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 状态机驱动器
/// </summary>
public sealed partial class TemplateWorkflowStateMachineDriver : TemplateWorkflowStateMachineDriverBase
{
    /// <inheritdoc cref="TemplateWorkflowStateMachineDriver"/>
    public TemplateWorkflowStateMachineDriver(IWorkflowBuilder<TemplateWorkflow> workflowBuilder, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
        : base(workflowBuilder, messageDispatcher, serviceProvider)
    {
    }
}
