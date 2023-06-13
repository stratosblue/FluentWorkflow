using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class SchedulerSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public SchedulerSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace};

/// <summary>
/// <see cref=""{WorkflowName}""/> 调度器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal abstract partial class {WorkflowName}SchedulerBase
    : WorkflowScheduler<{WorkflowName}, {WorkflowName}StateMachine, I{WorkflowName}>
{{
    /// <inheritdoc cref=""{WorkflowName}SchedulerBase""/>
    protected {WorkflowName}SchedulerBase(IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(messageDispatcher, serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override {WorkflowName}StateMachine CreateStateMachine({WorkflowName} workflow)
    {{
        return new {WorkflowName}StateMachine(workflow, MessageDispatcher, ServiceProvider);
    }}
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 调度器
/// </summary>
internal partial class {WorkflowName}Scheduler : {WorkflowName}SchedulerBase
{{
    /// <inheritdoc cref=""{WorkflowName}Scheduler""/>
    public {WorkflowName}Scheduler(IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(messageDispatcher, serviceProvider)
    {{
    }}
}}
");
        yield return new($"{WorkflowName}.Scheduler.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
