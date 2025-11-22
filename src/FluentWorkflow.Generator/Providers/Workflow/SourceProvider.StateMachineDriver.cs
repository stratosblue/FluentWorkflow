using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StateMachineDriverSourceProvider(GenerateContext generateContext)
    : WorkflowSourceProvider(generateContext)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(2048);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}.{WorkflowName}.Internal;

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的状态机驱动器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowClassName}StateMachineDriverBase
    : WorkflowStateMachineDriver<{WorkflowClassName}, {WorkflowClassName}Context, {WorkflowClassName}StateMachine, {WorkflowName}StageCompletedMessageBase, {WorkflowName}FailureMessage, I{WorkflowClassName}>
    , IWorkflowMessageHandler<{WorkflowName}StageCompletedMessageBase>
    , IWorkflowMessageHandler<{WorkflowName}FailureMessage>
    , IWorkflowResumer<{WorkflowClassName}>
    , I{WorkflowClassName}
{{
    /// <inheritdoc cref=""{WorkflowClassName}StateMachineDriverBase""/>
    protected {WorkflowClassName}StateMachineDriverBase(IWorkflowBuilder<{WorkflowClassName}> workflowBuilder, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
        : base(workflowBuilder, messageDispatcher, serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    public virtual async Task ResumeAsync(IWorkflowContext context, CancellationToken cancellationToken = default)
    {{
        if (context is not {WorkflowClassName}Context {{ }} typedContext)
        {{
            throw new WorkflowInvalidOperationException($""The context must be a instance of \""{{typeof({WorkflowClassName}Context)}}\""."");
        }}

        ThrowIfContextInvalid(context);

        var state = context.State.StageState;
        if (state == WorkflowStageState.Unknown)
        {{
            throw new WorkflowInvalidOperationException(""The stage state is invalid. Can not resume."");
        }}

        switch (state)
        {{
            case WorkflowStageState.Created:
                {{
                    var stateMachine = await RestoreStateMachineAsync(context, cancellationToken);
                    await stateMachine.MoveNextAsync(cancellationToken);
                    break;
                }}

            case WorkflowStageState.Scheduled:
                {{
                    var currentStage = context.Stage;
                    switch (currentStage)
                    {{
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
                        case {Names.WorkflowNameStagesClass}.{stage.Name}:
                            {{
                                var stageMessage = new {Names.MessageName(stage)}(WorkflowMessageIdProvider.Generate(), typedContext);
                                await MessageDispatcher.PublishAsync(stageMessage, cancellationToken);
                                break;
                            }}
");
        }
        builder.AppendLine($@"
                        default:
                            throw new WorkflowInvalidOperationException($""Unsupported scheduled stage：{{currentStage}}"");
                    }}
                    break;
                }}

            case WorkflowStageState.Finished:
                {{
                    var currentStage = context.Stage;
                    {WorkflowName}StageCompletedMessageBase stageCompletedMessage = currentStage switch
                    {{
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
                        {Names.WorkflowNameStagesClass}.{stage.Name} => new {Names.CompletedMessageName(stage)}(WorkflowMessageIdProvider.Generate(), typedContext),");
        }
        builder.AppendLine($@"
                        _ => throw new WorkflowInvalidOperationException($""Unsupported finished stage：{{currentStage}}""),
                    }};

                    await DoInputAsync(stageCompletedMessage, cancellationToken);
                    break;
                }}

            default:
                throw new WorkflowInvalidOperationException($""Unsupported stage state \""{{state}}\""."");
        }}
    }}

    /// <inheritdoc/>
    protected override async Task DoInputAsync({WorkflowName}StageCompletedMessageBase message, CancellationToken cancellationToken)
    {{
        var stateMachine = await RestoreStateMachineAsync(message.Context, cancellationToken);

        if (await stateMachine.SetStageCompletedAsync(message, cancellationToken))
        {{
            await InternalDriveAsync(stateMachine, cancellationToken);
        }}
    }}

    /// <inheritdoc/>
    protected override async Task DoInputAsync({WorkflowName}FailureMessage message, CancellationToken cancellationToken)
    {{
        var stateMachine = await RestoreStateMachineAsync(message.Context, cancellationToken);

        if (await stateMachine.SetFailedAsync(message, cancellationToken))
        {{
            await InternalDriveAsync(stateMachine, cancellationToken);
        }}
    }}

    /// <inheritdoc/>
    protected override bool ValidationContext(IWorkflowContext context) => {WorkflowName}Stages.StageIds.Contains(context.Stage);
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 状态机驱动器
/// </summary>
public sealed partial class {WorkflowClassName}StateMachineDriver : {WorkflowClassName}StateMachineDriverBase
{{
    /// <inheritdoc cref=""{WorkflowClassName}StateMachineDriver""/>
    public {WorkflowClassName}StateMachineDriver(IWorkflowBuilder<{WorkflowClassName}> workflowBuilder, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
        : base(workflowBuilder, messageDispatcher, serviceProvider)
    {{
    }}
}}
");

        yield return new($"Workflow.{WorkflowName}.StateMachineDriver.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
