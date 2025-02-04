using FluentWorkflow.Build;
using FluentWorkflow.RabbitMQ;
using FluentWorkflow.MessageDispatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;

namespace FluentWorkflow;

/// <summary>
///
/// </summary>
public static class FluentWorkflowRabbitMQServiceCollectionExtensions
{
    #region Public 方法

    /// <summary>
    /// 使用基于 RabbitMQ 的 <see cref="IWorkflowMessageDispatcher"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseRabbitMQMessageDispatcher(this IFluentWorkflowBuilder builder, Uri uri)
    {
        return builder.UseRabbitMQMessageDispatcher(options => options.Uri = uri);
    }

    /// <summary>
    /// 使用基于 RabbitMQ 的 <see cref="IWorkflowMessageDispatcher"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="connectionFactory"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseRabbitMQMessageDispatcher(this IFluentWorkflowBuilder builder, IConnectionFactory connectionFactory)
    {
        return builder.UseRabbitMQMessageDispatcher(options => options.ConnectionFactory = connectionFactory);
    }

    /// <summary>
    /// 使用基于 RabbitMQ 的 <see cref="IWorkflowMessageDispatcher"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="optionsSetup"></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseRabbitMQMessageDispatcher(this IFluentWorkflowBuilder builder, Action<RabbitMQOptions>? optionsSetup = null)
    {
        if (optionsSetup is not null)
        {
            builder.Services.Configure<RabbitMQOptions>(optionsSetup);
        }

        builder.Services.TryAddSingleton<IRabbitMQExchangeSelector, OptionsBasedRabbitMQExchangeSelector>();
        builder.Services.TryAddSingleton<IRabbitMQConnectionProvider, RabbitMQConnectionProvider>();
        builder.Services.TryAddSingleton<IRabbitMQChannelPool, RabbitMQChannelPool>();

        builder.Services.Replace(ServiceDescriptor.Singleton<IFluentWorkflowBootstrapper, RabbitMQBootstrapper>());
        builder.Services.Replace(ServiceDescriptor.Singleton<IWorkflowMessageDispatcher, RabbitMQWorkflowMessageDispatcher>());

        if (!builder.Properties.ContainsKey(typeof(IFluentWorkflowBootstrapper)))   //不重复添加
        {
            builder.Services.AddHostedService<BootstrapperHostedService>();
            builder.Properties.Add(typeof(IFluentWorkflowBootstrapper), typeof(RabbitMQBootstrapper));
        }

        return builder;
    }

    #endregion Public 方法
}
