﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using TemplateNamespace.Message;

namespace TemplateNamespace.Handler;

/// <summary>
/// <see cref="TemplateWorkflow"/> 的 <inheritdoc cref="WorkflowResultObserver{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}"/> 基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateWorkflowResultObserverBase
    : WorkflowResultObserver<TemplateWorkflow, TemplateWorkflowFinishedMessage, ITemplateWorkflow>
    , ITemplateWorkflow
{
    /// <inheritdoc cref="TemplateWorkflowResultObserverBase"/>
    protected TemplateWorkflowResultObserverBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override Task OnFinishedAsync(TemplateWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的 <inheritdoc cref="WorkflowResultObserver{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}"/>
/// </summary>
public partial class TemplateWorkflowResultObserver : TemplateWorkflowResultObserverBase
{
    /// <inheritdoc cref="TemplateWorkflowResultObserverBase"/>
    public TemplateWorkflowResultObserver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
