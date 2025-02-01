﻿// <Auto-Generated/>
using System.ComponentModel;
using FluentWorkflow.Build;
using FluentWorkflow.Scheduler;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TemplateNamespace;
using TemplateNamespace.Template;
using TemplateNamespace.Template.Handler;
using TemplateNamespace.Template.Internal;
using TemplateNamespace.Template.Message;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="TemplateWorkflow"/> 配置
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TemplateWorkflowConfiguration : ITemplateWorkflow
{
    /// <inheritdoc cref="IFluentWorkflowBuilder"/>
    public IFluentWorkflowBuilder Builder { get; }

    /// <inheritdoc cref="IServiceCollection"/>
    public IServiceCollection Services { get; }

    /// <summary>
    /// 最终使用的流程实现类型
    /// </summary>
    public abstract Type WorkflowType { get; }

    /// <inheritdoc cref="TemplateWorkflowConfiguration{TWorkflow}"/>
    internal TemplateWorkflowConfiguration(IFluentWorkflowBuilder builder)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        Services = builder.Services ?? throw new ArgumentNullException(nameof(builder.Services));
    }

    /// <summary>
    /// 添加 <see cref="TemplateWorkflow"/> 的调度器
    /// </summary>
    /// <returns></returns>
    public abstract TemplateWorkflowConfiguration AddScheduler();
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 配置
/// <br/>使用的流程实现为 <typeparamref name="TWorkflow"/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class TemplateWorkflowConfiguration<TWorkflow>
    : TemplateWorkflowConfiguration
    where TWorkflow : TemplateWorkflow
{
    /// <inheritdoc/>
    public override Type WorkflowType { get; } = typeof(TWorkflow);

    /// <inheritdoc cref="TemplateWorkflowConfiguration"/>
    internal TemplateWorkflowConfiguration(IFluentWorkflowBuilder builder) : base(builder)
    {
    }

    /// <summary>
    /// 添加 <see cref="TemplateWorkflow"/> 的调度器
    /// </summary>
    /// <returns></returns>
    public override TemplateWorkflowConfiguration AddScheduler()
    {
        var builder = Builder;

        #region StartRequestHandler

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateStartRequestHandler<TWorkflow>, TemplateStartRequestMessage, ITemplateWorkflow>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TemplateStartRequestHandler<TWorkflow>), typeof(TemplateStartRequestHandler<TWorkflow>), ServiceLifetime.Scoped));

        #endregion StartRequestHandler

        #region StateMachineDriver

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStateMachineDriver, StageStage1CAUKCompletedMessage, ITemplateWorkflow>();
        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStateMachineDriver, StageStage2BPTGCompletedMessage, ITemplateWorkflow>();
        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStateMachineDriver, StageStage3AWBNCompletedMessage, ITemplateWorkflow>();

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<TemplateWorkflow, TemplateWorkflowStateMachineDriver, TemplateFailureMessage, ITemplateWorkflow>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TemplateWorkflowStateMachineDriver), typeof(TemplateWorkflowStateMachineDriver), ServiceLifetime.Scoped));

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResumer<TemplateWorkflow>), static serviceProvider => serviceProvider.GetRequiredService<TemplateWorkflowStateMachineDriver>(), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResumer<TWorkflow>), static serviceProvider => serviceProvider.GetRequiredService<TemplateWorkflowStateMachineDriver>(), ServiceLifetime.Scoped));

        #endregion StateMachineDriver

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowScheduler<TemplateWorkflow>), typeof(TemplateWorkflowScheduler), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowScheduler<TWorkflow>), typeof(TemplateWorkflowScheduler), ServiceLifetime.Scoped));

        return this;
    }
}
