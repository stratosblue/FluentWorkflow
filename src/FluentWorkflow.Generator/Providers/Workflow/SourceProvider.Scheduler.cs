using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class SchedulerSourceProvider(GenerateContext generateContext)
    : WorkflowSourceProvider(generateContext)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}.{WorkflowName}.Internal;

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 调度器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal abstract partial class {WorkflowClassName}SchedulerBase
    : WorkflowScheduler<{WorkflowClassName}, {WorkflowClassName}StateMachine, I{WorkflowClassName}>
{{
    /// <inheritdoc cref=""{WorkflowClassName}SchedulerBase""/>
    protected {WorkflowClassName}SchedulerBase(IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(messageDispatcher, serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override {WorkflowClassName}StateMachine CreateStateMachine({WorkflowClassName} workflow)
    {{
        return new {WorkflowClassName}StateMachine(workflow, MessageDispatcher, ServiceProvider);
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 调度器
/// </summary>
internal partial class {WorkflowClassName}Scheduler : {WorkflowClassName}SchedulerBase
{{
    /// <inheritdoc cref=""{WorkflowClassName}Scheduler""/>
    public {WorkflowClassName}Scheduler(IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(messageDispatcher, serviceProvider)
    {{
    }}
}}
");
        yield return new($"Workflow.{WorkflowName}.Scheduler.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
