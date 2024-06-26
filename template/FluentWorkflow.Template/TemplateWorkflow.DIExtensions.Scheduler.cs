﻿// <Auto-Generated/>
using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TemplateNamespace;
using TemplateNamespace.Handler;
using TemplateNamespace.Internal;
using TemplateNamespace.Message;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TemplateWorkflowSchedulerDIExtensions
{
    /// <summary>
    /// 添加 <see cref="TemplateWorkflow"/> 的调度器
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder AddTemplateWorkflowScheduler(this IFluentWorkflowBuilder builder) => builder.AddTemplateWorkflowScheduler<TemplateWorkflow>();

    /// <summary>
    /// 添加 <see cref="TemplateWorkflow"/> 的调度器，使用 <typeparamref name="TWorkflow"/> 替代 <see cref="TemplateWorkflow"/>
    /// </summary>
    /// <typeparam name="TWorkflow"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder AddTemplateWorkflowScheduler<TWorkflow>(this IFluentWorkflowBuilder builder) where TWorkflow : TemplateWorkflow
    {
        builder.AddTemplateWorkflow<TWorkflow>();

        #region StartRequestHandler

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStartRequestHandler<TWorkflow>, TemplateWorkflowStartRequestMessage, ITemplateWorkflow>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TemplateWorkflowStartRequestHandler<TWorkflow>), typeof(TemplateWorkflowStartRequestHandler<TWorkflow>), ServiceLifetime.Scoped));

        #endregion StartRequestHandler

        #region StateMachineDriver

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStateMachineDriver, TemplateWorkflowStage1CAUKStageCompletedMessage, ITemplateWorkflow>();
        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStateMachineDriver, TemplateWorkflowStage2BPTGStageCompletedMessage, ITemplateWorkflow>();
        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStateMachineDriver, TemplateWorkflowStage3AWBNStageCompletedMessage, ITemplateWorkflow>();

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStateMachineDriver, TemplateWorkflowFailureMessage, ITemplateWorkflow>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TemplateWorkflowStateMachineDriver), typeof(TemplateWorkflowStateMachineDriver), ServiceLifetime.Scoped));

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResumer<TemplateWorkflow>), static serviceProvider => serviceProvider.GetRequiredService<TemplateWorkflowStateMachineDriver>(), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResumer<TWorkflow>), static serviceProvider => serviceProvider.GetRequiredService<TemplateWorkflowStateMachineDriver>(), ServiceLifetime.Scoped));

        #endregion StateMachineDriver

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowScheduler<TemplateWorkflow>), typeof(TemplateWorkflowScheduler), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowScheduler<TWorkflow>), typeof(TemplateWorkflowScheduler), ServiceLifetime.Scoped));

        return builder;
    }
}
