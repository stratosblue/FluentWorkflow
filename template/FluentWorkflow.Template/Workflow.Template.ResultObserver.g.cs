﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using TemplateNamespace.Template.Message;

namespace TemplateNamespace.Template.Handler;

/// <summary>
/// <see cref="TemplateWorkflow"/> 的 <inheritdoc cref="WorkflowResultObserver{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}"/> 基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateResultObserverBase
    : WorkflowResultObserver<TemplateWorkflow, TemplateFinishedMessage, ITemplateWorkflow>
    , ITemplateWorkflow
{
    /// <inheritdoc cref="TemplateResultObserverBase"/>
    protected TemplateResultObserverBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override Task OnFinishedAsync(TemplateFinishedMessage finishedMessage, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的 <inheritdoc cref="WorkflowResultObserver{TWorkflow, TWorkflowFinishedMessage, TWorkflowBoundary}"/>
/// </summary>
public partial class TemplateResultObserver : TemplateResultObserverBase
{
    /// <inheritdoc cref="TemplateResultObserverBase"/>
    public TemplateResultObserver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
