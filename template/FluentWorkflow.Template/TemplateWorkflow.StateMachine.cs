﻿// <Auto-Generated/>
using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Extensions;
using FluentWorkflow.Interface;
using TemplateNamespace.Message;

namespace TemplateNamespace
{
    partial class TemplateWorkflow
    {
        /// <summary>
        /// <see cref="TemplateWorkflow"/> 状态机基类
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract partial class TemplateWorkflowStateMachineBase
            : WorkflowStateMachine<ITemplateWorkflow>
            , ITemplateWorkflow
        {
            private readonly IWorkflowMessageDispatcher _messageDispatcher;

            /// <inheritdoc cref="TemplateWorkflowContext"/>
            protected readonly TemplateWorkflowContext TypedContext;

            /// <summary>
            /// 工作流程 <see cref="TemplateWorkflow"/> 实例
            /// </summary>
            protected readonly TemplateWorkflow Workflow;

            /// <inheritdoc cref="TemplateWorkflowStateMachineBase"/>
            public TemplateWorkflowStateMachineBase(TemplateWorkflow workflow, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
                : base(workflow.Context, messageDispatcher, serviceProvider)
            {
                Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
                TypedContext = workflow.Context;
                _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
            }

            /// <inheritdoc/>
            public override async Task MoveNextAsync(CancellationToken cancellationToken)
            {
                var currentStage = Context.Stage;
                if (string.IsNullOrWhiteSpace(currentStage))
                {
                    currentStage = TemplateWorkflowStages.Stage1CAUK;
                    Context.SetCurrentStage(currentStage);
                }

                switch (currentStage)
                {
                    case TemplateWorkflowStages.Stage1CAUK:
                        {
                            var stageMessage = new TemplateWorkflowStage1CAUKStageMessage(TypedContext);
                            await Workflow.OnStage1CAUKAsync(stageMessage, (message, cancellationToken) => PublishStageMessageAsync(message, cancellationToken), cancellationToken);
                        }
                        return;
                    case TemplateWorkflowStages.Stage2BPTG:
                        {
                            var stageMessage = new TemplateWorkflowStage2BPTGStageMessage(TypedContext);
                            await Workflow.OnStage2BPTGAsync(stageMessage, (message, cancellationToken) => PublishStageMessageAsync(message, cancellationToken), cancellationToken);
                        }
                        return;
                    case TemplateWorkflowStages.Stage3AWBN:
                        {
                            var stageMessage = new TemplateWorkflowStage3AWBNStageMessage(TypedContext);
                            await Workflow.OnStage3AWBNAsync(stageMessage, (message, cancellationToken) => PublishStageMessageAsync(message, cancellationToken), cancellationToken);
                        }
                        return;
                    case TemplateWorkflowStages.Failure:
                        {
                            Context.TryGetFailureMessage(out var failureMessage);
                            var finishedMessage = new TemplateWorkflowFinishedMessage(TypedContext, false, failureMessage ?? "Unknown error");
                            await _messageDispatcher.PublishAsync(finishedMessage, cancellationToken);
                        }
                        return;
                    case TemplateWorkflowStages.Completion:
                        {
                            await Workflow.OnCompletionAsync(TypedContext, cancellationToken);

                            if (Context.Flag.HasFlag(WorkflowFlag.IsBeenAwaited)
                                || !Context.Flag.HasFlag(WorkflowFlag.NotNotifyOnFinish))
                            {
                                var finishedMessage = new TemplateWorkflowFinishedMessage(TypedContext, true, "SUCCESS");
                                await _messageDispatcher.PublishAsync(finishedMessage, cancellationToken);
                            }
                        }
                        return;
                }
                throw new WorkflowInvalidOperationException($"未知的阶段：{currentStage}");
            }

            /// <inheritdoc/>
            public override Task<bool> IsCompletedAsync(CancellationToken cancellationToken)
            {
                var currentStage = Context.Stage;
                var result = string.Equals(TemplateWorkflowStages.Completion, currentStage);

                return Task.FromResult(result);
            }

            /// <summary>
            /// 使用完成消息设置阶段完成
            /// </summary>
            /// <param name="stageCompletedMessage"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            /// <exception cref="WorkflowInvalidOperationException"></exception>
            internal virtual Task SetStageCompletedAsync(ITemplateWorkflowStageCompletedMessage stageCompletedMessage, CancellationToken cancellationToken)
            {
                return stageCompletedMessage switch
                {
                    TemplateWorkflowStage1CAUKStageCompletedMessage stage1CA5CStageCompletedMessage => Workflow.OnStage1CAUKCompletedAsync(stage1CA5CStageCompletedMessage, (message, cancellationToken) => OnStageCompletedAsync(message, cancellationToken), cancellationToken),
                    TemplateWorkflowStage2BPTGStageCompletedMessage stage2B74EStageCompletedMessage => Workflow.OnStage2BPTGCompletedAsync(stage2B74EStageCompletedMessage, (message, cancellationToken) => OnStageCompletedAsync(message, cancellationToken), cancellationToken),
                    TemplateWorkflowStage3AWBNStageCompletedMessage stage3A2B4StageCompletedMessage => Workflow.OnStage3AWBNCompletedAsync(stage3A2B4StageCompletedMessage, (message, cancellationToken) => OnStageCompletedAsync(message, cancellationToken), cancellationToken),
                    _ => throw new WorkflowInvalidOperationException($"未知的阶段完成消息：{stageCompletedMessage}"),
                };
            }

            /// <inheritdoc/>
            protected override Task OnStageCompletedAsync<ITemplateWorkflowStageCompletedMessage>(ITemplateWorkflowStageCompletedMessage stageCompletedMessage, CancellationToken cancellationToken)
            {
                switch (stageCompletedMessage.Stage)
                {
                    case TemplateWorkflowStages.Stage1CAUK:
                        {
                            Context.SetCurrentStage(TemplateWorkflowStages.Stage2BPTG);
                        }
                        break;
                    case TemplateWorkflowStages.Stage2BPTG:
                        {
                            Context.SetCurrentStage(TemplateWorkflowStages.Stage3AWBN);
                        }
                        break;
                    case TemplateWorkflowStages.Stage3AWBN:
                        {
                            Context.SetCurrentStage(TemplateWorkflowStages.Completion);
                        }
                        break;
                    default:
                        throw new WorkflowInvalidOperationException($"未知的阶段完成消息：{stageCompletedMessage}");
                }
                return base.OnStageCompletedAsync(stageCompletedMessage, cancellationToken);
            }

            /// <summary>
            /// 使用失败消息设置流程失败
            /// </summary>
            /// <param name="failureMessage"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            /// <exception cref="WorkflowInvalidOperationException"></exception>
            internal virtual Task SetFailedAsync(ITemplateWorkflowFailureMessage failureMessage, CancellationToken cancellationToken)
            {
                if (failureMessage is not TemplateWorkflowFailureMessage typedFailureMessage)
                {
                    throw new WorkflowInvalidOperationException($"未知的失败消息：{failureMessage}");
                }
                return Workflow.OnFailedAsync(typedFailureMessage, (message, cancellationToken) => OnFailedAsync(message, cancellationToken), cancellationToken);
            }

            /// <summary>
            /// 在工作流程失败时
            /// </summary>
            /// <param name="failureMessage"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            protected virtual Task OnFailedAsync(ITemplateWorkflowFailureMessage failureMessage, CancellationToken cancellationToken)
            {
                Context.SetFailureMessage(failureMessage.Message);
                Context.SetFailureStackTrace(failureMessage.RemoteStackTrace);
                Context.SetValue(FluentWorkflowConstants.ContextKeys.FailureStage, failureMessage.Stage);

                Context.SetCurrentStage(TemplateWorkflowStages.Failure);

                return Task.CompletedTask;
            }
        }
    }
}

namespace TemplateNamespace.Internal
{
    /// <summary>
    /// <see cref="TemplateWorkflow"/> 的状态机
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed partial class TemplateWorkflowStateMachine : TemplateWorkflow.TemplateWorkflowStateMachineBase
    {
        /// <inheritdoc/>
        public TemplateWorkflowStateMachine(TemplateWorkflow workflow, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(workflow, messageDispatcher, serviceProvider)
        {
        }
    }
}
