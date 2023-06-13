﻿using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class ResultObserverSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public ResultObserverSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}.Handler;

/// <summary>
/// <see cref=""{WorkflowName}""/> 的 <inheritdoc cref=""WorkflowResultObserver{{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}}""/> 基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowName}ResultObserverBase
    : WorkflowResultObserver<{WorkflowName}, {WorkflowName}FinishedMessage, I{WorkflowName}>
    , I{WorkflowName}
{{
    /// <inheritdoc cref=""{WorkflowName}ResultObserverBase""/>
    protected {WorkflowName}ResultObserverBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override Task OnFinishedAsync({WorkflowName}FinishedMessage finishedMessage, CancellationToken cancellationToken) => Task.CompletedTask;
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 的 <inheritdoc cref=""WorkflowResultObserver{{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}}""/>
/// </summary>
public partial class {WorkflowName}ResultObserver : {WorkflowName}ResultObserverBase
{{
    /// <inheritdoc cref=""{WorkflowName}ResultObserverBase""/>
    public {WorkflowName}ResultObserver(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}
}}
");
        yield return new($"{WorkflowName}.ResultObserver.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
