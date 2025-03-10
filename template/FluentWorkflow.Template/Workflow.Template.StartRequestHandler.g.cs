﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Scheduler;
using TemplateNamespace.Template.Message;

namespace TemplateNamespace.Template.Handler;

/// <summary>
/// <see cref="TemplateWorkflow"/> 的启动请求处理器
/// </summary>
/// <typeparam name="TWorkflow">用以启动的工作流程具体实现（<see cref="TemplateWorkflow"/> 或其派生类型）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class TemplateStartRequestHandler<TWorkflow>
    : WorkflowStartRequestHandler<TWorkflow, TemplateWorkflowContext, TemplateStartRequestMessage, ITemplateWorkflow>
    , ITemplateWorkflow
    where TWorkflow : TemplateWorkflow
{
    /// <inheritdoc cref="IServiceProvider"/>
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc cref="TemplateStartRequestHandler{TWorkflow}"/>
    public TemplateStartRequestHandler(IWorkflowBuilder<TWorkflow> workflowBuilder, IWorkflowScheduler<TWorkflow> workflowScheduler, IServiceProvider serviceProvider) : base(workflowBuilder, workflowScheduler)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
}
