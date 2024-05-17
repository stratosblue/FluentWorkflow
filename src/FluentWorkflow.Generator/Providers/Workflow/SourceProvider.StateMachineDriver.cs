using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StateMachineDriverSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public StateMachineDriverSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(2048);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}.Internal;

/// <summary>
/// <see cref=""{WorkflowName}""/> 的状态机驱动器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowName}StateMachineDriverBase
    : WorkflowStateMachineDriver<{WorkflowName}, {WorkflowName}Context, {WorkflowName}StateMachine, {WorkflowName}StageCompletedMessageBase, {WorkflowName}FailureMessage, I{WorkflowName}>
    , IWorkflowMessageHandler<{WorkflowName}StageCompletedMessageBase>
    , IWorkflowMessageHandler<{WorkflowName}FailureMessage>
    , I{WorkflowName}
{{
    /// <inheritdoc cref=""{WorkflowName}StateMachineDriverBase""/>
    protected {WorkflowName}StateMachineDriverBase(IWorkflowBuilder<{WorkflowName}> workflowBuilder, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
        : base(workflowBuilder, messageDispatcher, serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override async Task DoInputAsync({WorkflowName}StageCompletedMessageBase message, CancellationToken cancellationToken)
    {{
        var stateMachine = await RestoreStateMachineAsync(message, cancellationToken);

        if (await stateMachine.SetStageCompletedAsync(message, cancellationToken))
        {{
            await InternalDriveAsync(stateMachine, cancellationToken);
        }}
    }}

    /// <inheritdoc/>
    protected override async Task DoInputAsync({WorkflowName}FailureMessage message, CancellationToken cancellationToken)
    {{
        var stateMachine = await RestoreStateMachineAsync(message, cancellationToken);

        if (await stateMachine.SetFailedAsync(message, cancellationToken))
        {{
            await InternalDriveAsync(stateMachine, cancellationToken);
        }}
    }}
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 状态机驱动器
/// </summary>
public sealed partial class {WorkflowName}StateMachineDriver : {WorkflowName}StateMachineDriverBase
{{
    /// <inheritdoc cref=""{WorkflowName}StateMachineDriver""/>
    public {WorkflowName}StateMachineDriver(IWorkflowBuilder<{WorkflowName}> workflowBuilder, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
        : base(workflowBuilder, messageDispatcher, serviceProvider)
    {{
    }}
}}
");

        yield return new($"{WorkflowName}.StateMachineDriver.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
