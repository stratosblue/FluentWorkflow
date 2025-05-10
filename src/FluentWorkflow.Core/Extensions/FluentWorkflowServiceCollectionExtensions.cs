using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Build;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Handler;
using FluentWorkflow.MessageDispatch;
using FluentWorkflow.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentWorkflow;

/// <summary>
///
/// </summary>
public static class FluentWorkflowServiceCollectionExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加 Debug 运行器，以使用 <see cref="IWorkflowDebugRunner"/> 进行流程调试
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder AddDebugRunner(this IFluentWorkflowBuilder builder)
    {
        builder.Services.TryAddSingleton<IWorkflowDebugRunner, WorkflowDebugRunner>();

        FluentWorkflowDebugUtil.EnableDebugActivityListen();

        return builder;
    }

    /// <summary>
    /// 添加 FluentWorkflow 基础组件
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder AddFluentWorkflow(this IServiceCollection services)
    {
        services.AddOptions<FluentWorkflowOptions>();

        services.TryAdd(ServiceDescriptor.Describe(typeof(IObjectSerializer), typeof(SystemTextJsonObjectSerializer), ServiceLifetime.Singleton));

        services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowDiagnosticSource), typeof(NoopWorkflowDiagnosticSource), ServiceLifetime.Singleton));

        services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowAwaitProcessor), typeof(SingleflowWorkflowAwaitProcessor), ServiceLifetime.Singleton));

        services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowContinuatorHub), typeof(WorkflowContinuatorHub), ServiceLifetime.Scoped));

        services.TryAddSingleton<IMessageConsumeDispatcher, MessageConsumeDispatcher>();

        return new FluentWorkflowBuilder(services);
    }

    /// <summary>
    /// 启用诊断
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder EnableDiagnostic(this IFluentWorkflowBuilder builder)
    {
        builder.Services.RemoveAll<IWorkflowDiagnosticSource>();
        builder.Services.Add(ServiceDescriptor.Describe(typeof(IWorkflowDiagnosticSource), typeof(WorkflowDiagnosticSource), ServiceLifetime.Singleton));
        return builder;
    }

    /// <summary>
    /// 使用基于内存的 <see cref="IWorkflowMessageDispatcher"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="optionsSetup"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseInMemoryWorkflowMessageDispatcher(this IFluentWorkflowBuilder builder, Action<InMemoryWorkflowMessageDispatcherOptions>? optionsSetup = null)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IWorkflowMessageDispatcher, InMemoryWorkflowMessageDispatcher>());

        builder.Services.AddOptions<InMemoryWorkflowMessageDispatcherOptions>();
        if (optionsSetup != null)
        {
            builder.Services.Configure(optionsSetup);
        }

        return builder;
    }

    /// <summary>
    /// 使用指定的 <typeparamref name="TWorkflowAwaitProcessor"/> 作为全局 <see cref="IWorkflowAwaitProcessor"/>
    /// </summary>
    /// <typeparam name="TWorkflowAwaitProcessor"></typeparam>
    /// <param name="builder"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseWorkflowAwaitProcessor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWorkflowAwaitProcessor>(this IFluentWorkflowBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        where TWorkflowAwaitProcessor : class, IWorkflowAwaitProcessor
    {
        builder.Services.RemoveAll<IWorkflowAwaitProcessor>();
        builder.Services.Add(ServiceDescriptor.Describe(typeof(IWorkflowAwaitProcessor), typeof(TWorkflowAwaitProcessor), serviceLifetime));
        return builder;
    }

    #endregion Public 方法
}
