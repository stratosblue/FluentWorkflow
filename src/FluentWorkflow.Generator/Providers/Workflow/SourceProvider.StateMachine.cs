using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StateMachineSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public StateMachineSourceProvider(GenerateContext context) : base(context)
    {
    }

    #endregion Public 构造函数

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
    partial class {WorkflowName}
    {{
        /// <summary>
        /// <see cref=""{WorkflowName}""/> 状态机基类
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract partial class {WorkflowName}StateMachineBase
            : WorkflowStateMachine<I{WorkflowName}>
            , I{WorkflowName}
        {{
            private readonly IWorkflowMessageDispatcher _messageDispatcher;

            /// <inheritdoc cref=""{WorkflowName}Context""/>
            protected readonly {WorkflowName}Context TypedContext;

            /// <summary>
            /// 工作流程 <see cref=""{WorkflowName}""/> 实例
            /// </summary>
            protected readonly {WorkflowName} Workflow;

            /// <inheritdoc cref=""{WorkflowName}StateMachineBase""/>
            public {WorkflowName}StateMachineBase({WorkflowName} workflow, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
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
                }}

                using var singleCaller = new ScopePublishStageMessageSingleCaller(MessageDispatcher);

                switch (currentStage)
                {{
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"case {Names.WorkflowNameStagesClass}.{stage.Name}:
                    {{
                        var stageMessage = new {Names.MessageName(stage)}(TypedContext);
                        await Workflow.On{stage.Name}Async(stageMessage, singleCaller.PublishStageMessageAsync, cancellationToken);
                        return;
                    }}");
        }

        builder.AppendLine($@"
                    case {WorkflowName}Stages.Failure:
                        {{
                            Context.TryGetFailureMessage(out var failureMessage);
                            var finishedMessage = new {WorkflowName}FinishedMessage(TypedContext, false, failureMessage);
                            await _messageDispatcher.PublishAsync(finishedMessage, cancellationToken);
                            return;
                        }}
                    case {WorkflowName}Stages.Completion:
                        {{
                            await Workflow.OnCompletionAsync(TypedContext, cancellationToken);

                            if (Context.Flag.HasFlag(WorkflowFlag.IsBeenAwaited)
                                || !Context.Flag.HasFlag(WorkflowFlag.NotNotifyOnFinish))
                            {{
                                var finishedMessage = new {WorkflowName}FinishedMessage(TypedContext, true, ""SUCCESS"");
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

                return singleCaller.HasInvoked;
            }}

            /// <inheritdoc/>
            protected override Task OnStageCompletedAsync<I{WorkflowName}StageCompletedMessage>(I{WorkflowName}StageCompletedMessage stageCompletedMessage, CancellationToken cancellationToken)
            {{
                switch (stageCompletedMessage.Stage)
                {{
");
        for (int i = 0; i < Context.Stages.Length - 1; i++)
        {
            var stage = Context.Stages[i];
            var nextStage = Context.Stages[i + 1];

            builder.AppendLine($@"
            case {Names.WorkflowNameStagesClass}.{stage.Name}:
                {{
                    Context.SetCurrentStage({Names.WorkflowNameStagesClass}.{nextStage.Name});
                    break;
                }}");
        }

        var lastStage = Context.Stages.Last();

        builder.AppendLine($@"
                    case {Names.WorkflowNameStagesClass}.{lastStage.Name}:
                        {{
                            Context.SetCurrentStage({Names.WorkflowNameStagesClass}.{Names.WorkflowCompletionStageConstantName});
                            break;
                        }}
                    default:
                        throw new WorkflowInvalidOperationException($""未知的阶段完成消息：{{stageCompletedMessage}}"");
                }}
                return base.OnStageCompletedAsync(stageCompletedMessage, cancellationToken);
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

namespace {NameSpace}.Internal
{{
    /// <summary>
    /// <see cref=""{WorkflowName}""/> 的状态机
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed partial class {WorkflowName}StateMachine : {WorkflowName}.{WorkflowName}StateMachineBase
    {{
        /// <inheritdoc/>
        public {WorkflowName}StateMachine({WorkflowName} workflow, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(workflow, messageDispatcher, serviceProvider)
        {{
        }}
    }}
}}
");
        yield return new($"{WorkflowName}.StateMachine.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
