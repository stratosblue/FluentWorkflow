using System.ComponentModel;
using FluentWorkflow.Build;
using FluentWorkflow.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace FluentWorkflow.GenericExtension.TemplateNamespace;

/// <summary>
/// 基于 Redis 的 <see cref="IWorkflowAwaitProcessor"/> 配置
/// </summary>
public class RedisWorkflowAwaitProcessorOptions
{
    #region Public 属性

    /// <summary>
    /// 完成项的过期延时
    /// </summary>
    public TimeSpan FinishedItemExpireDelay { get; set; } = TimeSpan.FromDays(2);

    /// <summary>
    /// Key前缀
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    #endregion Public 属性
}

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RedisWorkflowAwaitProcessorDIExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加默认的基于 Redis 的子工作流程等待处理器
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <param name="optionsSetup"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseRedisWorkflowAwaitProcessor(this IFluentWorkflowBuilder builder, string configuration, Action<RedisWorkflowAwaitProcessorOptions>? optionsSetup = null)
    {
        var connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);
        return builder.UseRedisWorkflowAwaitProcessor(connectionMultiplexer, optionsSetup);
    }

    /// <summary>
    /// 添加默认的基于 Redis 的子工作流程等待处理器
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="connectionMultiplexer"></param>
    /// <param name="optionsSetup"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseRedisWorkflowAwaitProcessor(this IFluentWorkflowBuilder builder, IConnectionMultiplexer connectionMultiplexer, Action<RedisWorkflowAwaitProcessorOptions>? optionsSetup = null)
    {
        var provider = new FluentWorkflowRedisConnectionProvider(connectionMultiplexer);

        builder.Services.Replace(ServiceDescriptor.Describe(typeof(IFluentWorkflowRedisConnectionProvider), m => provider, ServiceLifetime.Singleton));
        builder.Services.Replace(ServiceDescriptor.Singleton<IWorkflowAwaitProcessor, RedisWorkflowAwaitProcessor>());
        if (optionsSetup is not null)
        {
            builder.Services.Configure(optionsSetup);
        }
        return builder;
    }

    /// <summary>
    /// 添加默认的基于 Redis 的子工作流程等待处理器
    /// </summary>
    /// <typeparam name="TConnectionProvider">连接提供器实现类型</typeparam>
    /// <param name="builder"></param>
    /// <param name="optionsSetup"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseRedisWorkflowAwaitProcessor<TConnectionProvider>(this IFluentWorkflowBuilder builder, Action<RedisWorkflowAwaitProcessorOptions>? optionsSetup = null)
        where TConnectionProvider : class, IFluentWorkflowRedisConnectionProvider
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IFluentWorkflowRedisConnectionProvider, TConnectionProvider>());
        builder.Services.Replace(ServiceDescriptor.Singleton<IWorkflowAwaitProcessor, RedisWorkflowAwaitProcessor>());
        if (optionsSetup is not null)
        {
            builder.Services.Configure(optionsSetup);
        }
        return builder;
    }

    #endregion Public 方法
}
