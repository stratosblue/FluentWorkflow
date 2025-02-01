﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Scheduler;

namespace TemplateNamespace.Template.Internal;

/// <summary>
/// <see cref="TemplateWorkflow"/> 调度器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal abstract partial class TemplateWorkflowSchedulerBase
    : WorkflowScheduler<TemplateWorkflow, TemplateWorkflowStateMachine, ITemplateWorkflow>
{
    /// <inheritdoc cref="TemplateWorkflowSchedulerBase"/>
    protected TemplateWorkflowSchedulerBase(IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(messageDispatcher, serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override TemplateWorkflowStateMachine CreateStateMachine(TemplateWorkflow workflow)
    {
        return new TemplateWorkflowStateMachine(workflow, MessageDispatcher, ServiceProvider);
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 调度器
/// </summary>
internal partial class TemplateWorkflowScheduler : TemplateWorkflowSchedulerBase
{
    /// <inheritdoc cref="TemplateWorkflowScheduler"/>
    public TemplateWorkflowScheduler(IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider) : base(messageDispatcher, serviceProvider)
    {
    }
}
