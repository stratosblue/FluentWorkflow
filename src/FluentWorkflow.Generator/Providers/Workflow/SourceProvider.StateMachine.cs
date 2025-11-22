using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StateMachineSourceProvider(GenerateContext context)
    : WorkflowSourceProvider(context)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        if (Context.Stages.IsDefaultOrEmpty)
        {
            yield break;
        }

        var builder = new StringBuilder(2048);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}
{{
    partial class {WorkflowClassName}
    {{
        /// <summary>
        /// <see cref=""{WorkflowClassName}""/> 状态机基类
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract partial class StateMachineBase
            : WorkflowStateMachine<I{WorkflowClassName}>
            , I{WorkflowClassName}
        {{
            private readonly IWorkflowMessageDispatcher _messageDispatcher;

            /// <inheritdoc cref=""{WorkflowClassName}Context""/>
            protected readonly {WorkflowClassName}Context TypedContext;

            /// <summary>
            /// 工作流程 <see cref=""{WorkflowClassName}""/> 实例
            /// </summary>
            protected readonly {WorkflowClassName} Workflow;

            /// <inheritdoc cref=""StateMachineBase""/>
            public StateMachineBase({WorkflowClassName} workflow, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
                : base(workflow.Context, messageDispatcher, serviceProvider)
            {{
                Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
                TypedContext = workflow.Context;
                _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
            }}

            /// <inheritdoc/>
            public override async Task MoveNextAsync(CancellationToken cancellationToken)
            {{
                var currentStage = Context.Stage;
                if (string.IsNullOrWhiteSpace(currentStage))
                {{
                    currentStage = {WorkflowName}Stages.{Context.Stages.First().Name};
                    Context.SetCurrentStage(currentStage);
                    if (!await Workflow.OnStartingAsync(TypedContext, cancellationToken))
                    {{
                        return;
                    }}
                }}

                using var singleCaller = new ScopePublishStageMessageSingleCaller(MessageDispatcher);

                switch (currentStage)
                {{
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"case {Names.WorkflowNameStagesClass}.{stage.Name}:
                    {{
                        var stageMessage = new {Names.MessageName(stage)}(WorkflowMessageIdProvider.Generate(), TypedContext);
                        stageMessage.Context.State.SetStageState(WorkflowStageState.Created);
                        await Workflow.On{stage.Name}Async(stageMessage, singleCaller.PublishStageMessageAsync, cancellationToken);
                        return;
                    }}
");
        }

        builder.AppendLine($@"
                    case {WorkflowName}Stages.Failure:
                        {{
                            var failureInformation = Context.GetFailureInformation();
                            var finishedMessage = new global::{NameSpace}.{WorkflowName}.Message.{WorkflowName}FinishedMessage(WorkflowMessageIdProvider.Generate(), TypedContext, false, failureInformation?.Message ?? ""Unknown error"");
                            await _messageDispatcher.PublishAsync(finishedMessage, cancellationToken);
                            return;
                        }}
                    case {WorkflowName}Stages.Completion:
                        {{
                            TypedContext.State.SetStageState(WorkflowStageState.Created);
                            await Workflow.OnCompletionAsync(TypedContext, cancellationToken);

                            if (Context.Flag.HasFlag(WorkflowFlag.IsBeenAwaited)
                                || !Context.Flag.HasFlag(WorkflowFlag.NotNotifyOnFinish))
                            {{
                                var finishedMessage = new global::{NameSpace}.{WorkflowName}.Message.{WorkflowName}FinishedMessage(WorkflowMessageIdProvider.Generate(), TypedContext, true, ""SUCCESS"");
                                await _messageDispatcher.PublishAsync(finishedMessage, cancellationToken);
                            }}
                            return;
                        }}
                }}
                throw new WorkflowInvalidOperationException($""未知的阶段：{{currentStage}}"");
            }}

            /// <inheritdoc/>
            public override Task<bool> IsCompletedAsync(CancellationToken cancellationToken)
            {{
                var currentStage = Context.Stage;
                var result = string.Equals({WorkflowName}Stages.Completion, currentStage);

                return Task.FromResult(result);
            }}

            /// <summary>
            /// 使用完成消息设置阶段完成
            /// </summary>
            /// <param name=""stageCompletedMessage""></param>
            /// <param name=""cancellationToken""></param>
            /// <returns>是否执行后续代码</returns>
            /// <exception cref=""WorkflowInvalidOperationException""></exception>
            internal virtual async Task<bool> SetStageCompletedAsync(I{WorkflowName}StageCompletedMessage stageCompletedMessage, CancellationToken cancellationToken)
            {{
                //设置上下文阶段状态，以使 OnStageCompletedAsync 中获取到的上下文当前阶段状态为已结束
                //如果在 OnStageCompletedAsync 中挂起上下文，在恢复流程时使用此值确定应当再次调用 SetStageCompletedAsync 而不是 MoveNextAsync
                stageCompletedMessage.Context.State.SetStageState(WorkflowStageState.Finished);

                using var singleCaller = new ScopeOnStageCompletedSingleCaller(this);

                var stageCompletedTask = stageCompletedMessage switch
                {{
");

        foreach (var stage in Context.Stages)
        {
            var stageCompletedMessageName = Names.CompletedMessageName(stage);
            var stageCompletedMessageVarName = stageCompletedMessageName.ToVarName();
            builder.AppendLine($"{stageCompletedMessageName} {stageCompletedMessageVarName} => Workflow.On{stage.Name}CompletedAsync({stageCompletedMessageVarName}, singleCaller.OnStageCompletedAsync, cancellationToken),");
        }

        builder.AppendLine($@"
                    _ => throw new WorkflowInvalidOperationException($""未知的阶段完成消息：{{stageCompletedMessage}}""),
                }};

                await stageCompletedTask;

                if (singleCaller.HasInvoked)
                {{
                    SetCurrentStageToNext();
                    return true;
                }}
                return false;

                void SetCurrentStageToNext()
                {{
                    var nextStage = stageCompletedMessage.Stage switch
                    {{
");
        for (int i = 0; i < Context.Stages.Length - 1; i++)
        {
            var stage = Context.Stages[i];
            var nextStage = Context.Stages[i + 1];

            builder.AppendLine($@"
                        {Names.WorkflowNameStagesClass}.{stage.Name} => {Names.WorkflowNameStagesClass}.{nextStage.Name},
");
        }

        var lastStage = Context.Stages.Last();

        builder.AppendLine($@"
                        {Names.WorkflowNameStagesClass}.{lastStage.Name} => {Names.WorkflowNameStagesClass}.{Names.WorkflowCompletionStageConstantName},
                        _ => throw new WorkflowInvalidOperationException($""未知的阶段完成消息：{{stageCompletedMessage}}""),
                    }};

                    Context.SetCurrentStage(nextStage);
                }}
            }}

            /// <summary>
            /// 使用失败消息设置流程失败
            /// </summary>
            /// <param name=""failureMessage""></param>
            /// <param name=""cancellationToken""></param>
            /// <returns>是否执行后续代码</returns>
            /// <exception cref=""WorkflowInvalidOperationException""></exception>
            internal virtual async Task<bool> SetFailedAsync(I{WorkflowName}FailureMessage failureMessage, CancellationToken cancellationToken)
            {{
                if (failureMessage is not {WorkflowName}FailureMessage typedFailureMessage)
                {{
                    throw new WorkflowInvalidOperationException($""未知的失败消息：{{failureMessage}}"");
                }}

                using var singleCaller = new ScopeOnFailedSingleCaller(this);

                await Workflow.OnFailedAsync(typedFailureMessage, singleCaller.OnFailedAsync, cancellationToken);

                return singleCaller.HasInvoked;
            }}

            /// <inheritdoc/>
            protected override async Task OnFailedAsync<TFailureMessage>(TFailureMessage failureMessage, CancellationToken cancellationToken)
            {{
                await base.OnFailedAsync(failureMessage, cancellationToken);

                Context.SetCurrentStage({WorkflowName}Stages.Failure);
            }}
        }}
    }}
}}

namespace {NameSpace}.{WorkflowName}.Internal
{{
    /// <summary>
    /// <see cref=""{WorkflowClassName}""/> 的状态机
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed partial class {WorkflowClassName}StateMachine : {WorkflowClassName}.StateMachineBase
    {{
        /// <inheritdoc/>
        public {WorkflowClassName}StateMachine({WorkflowClassName} workflow, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(workflow, messageDispatcher, serviceProvider)
        {{
        }}
    }}
}}
");
        yield return new($"Workflow.{WorkflowName}.StateMachine.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
