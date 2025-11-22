using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class ResultObserverSourceProvider(GenerateContext generateContext)
    : WorkflowSourceProvider(generateContext)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}.{WorkflowName}.Handler;

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的 <inheritdoc cref=""WorkflowResultObserver{{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}}""/> 基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowName}ResultObserverBase
    : WorkflowResultObserver<{WorkflowClassName}, global::{NameSpace}.{WorkflowName}.Message.{WorkflowName}FinishedMessage, I{WorkflowClassName}>
    , I{WorkflowClassName}
{{
    /// <inheritdoc cref=""{WorkflowName}ResultObserverBase""/>
    protected {WorkflowName}ResultObserverBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override Task OnFinishedAsync(global::{NameSpace}.{WorkflowName}.Message.{WorkflowName}FinishedMessage finishedMessage, CancellationToken cancellationToken) => Task.CompletedTask;
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的 <inheritdoc cref=""WorkflowResultObserver{{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}}""/>
/// </summary>
public partial class {WorkflowName}ResultObserver : {WorkflowName}ResultObserverBase
{{
    /// <inheritdoc cref=""{WorkflowName}ResultObserverBase""/>
    public {WorkflowName}ResultObserver(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}
}}
");
        yield return new($"Workflow.{WorkflowName}.ResultObserver.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
