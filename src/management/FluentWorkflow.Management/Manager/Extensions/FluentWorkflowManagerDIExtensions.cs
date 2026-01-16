#pragma warning disable IDE0130

using System.Net;
using FluentWorkflow.Management;
using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Manager.Communication;
using FluentWorkflow.Management.Manager.MessageHandler;
using FluentWorkflow.Management.Shared;
using FluentWorkflow.Management.Shared.Messages;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 管理器DI拓展
/// </summary>
public static class FluentWorkflowManagerDIExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加FluentWorkflow管理功能的管理器支持
    /// </summary>
    /// <param name="services"></param>
    /// <param name="endPoint"></param>
    /// <param name="cookie"></param>
    /// <param name="appName"></param>
    /// <returns></returns>
    public static IServiceCollection AddFluentWorkflowManagementManager(this IServiceCollection services, EndPoint endPoint, string cookie, string appName = "default")
    {
        ArgumentNullException.ThrowIfNull(endPoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(cookie);
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);

        services.AddHostedService<ManagerBootstrapBackgroundService>();

        services.AddOptions<ManagementGlobalOptions>()
                .Configure(options =>
                {
                    options.RegisterAppName(appName);
                });

        services.TryAddKeyedSingleton<ManagementManager>(appName);
        services.TryAddSingleton<ManagementManagerHub>();

        services.AddOptions<ManagementManagerOptions>(appName)
                .Configure(options =>
                {
                    options.Cookie = cookie;
                });

        var idleTimeout = TimeSpan.FromMinutes(2);

#if DEBUG
        idleTimeout = TimeSpan.FromHours(2);
#endif

        services.AddHoarwell(appName)
                .UseDefaultStreamApplication()
                .UseDefaultSocketTransportServer(options => options.EndPoints.Add(endPoint))
                .ConfigureMessageSerialization()
                .ConfigureInboundPipeline(pipelineBuilder =>
                {
                    pipelineBuilder.Use<ManagerInboundPipelineConfigureMiddleware>()
                                   .UsePipeReaderAdaptMiddleware()
                                   .UseUInt32LengthFieldBasedFrameDecoder()
                                   .UseDefaultMessageDeserializer()
                                   .UseInboundIdleStateHandler(idleTimeout)
                                   .RunEndpoint(endpointBuilder =>
                                   {
                                       //TODO 在此处配置更多的需要处理的消息
                                       endpointBuilder.HandleMessage<Hello, HelloMessageHandler>();
                                       endpointBuilder.GenericAck<Pong>();
                                       endpointBuilder.HandleMessage<Ping, PingPongMessageHandler>();
                                       endpointBuilder.GenericAck<ConsumptionControlResult>();
                                       endpointBuilder.GenericAck<MessageListQueryResponse>();
                                       endpointBuilder.GenericAck<MessageQueryResponse>();
                                       endpointBuilder.GenericAck<WorkerStatistics>();
                                       endpointBuilder.HandleMessage<WorkerStatusReport, WorkerStatusReportMessageHandler>();

                                       endpointBuilder.OnMessage<Close>((context, message) =>
                                       {
                                           context.Services.GetService<ILogger<ManagementManager>>()
                                                           ?.LogWarning("Connection closing by client: {Reason}", message.Data.Reason);
                                           return Task.CompletedTask;
                                       });
                                   });
                })
                .ConfigureOutboundPipeline(pipelineBuilder =>
                {
                    pipelineBuilder.UseUInt32LengthFieldBasedFrameEncoder()
                                   .UseOutboundIdleStateHandler(idleTimeout)
                                   .RunDefaultMessageSerializer();
                });

        return services;
    }

    #endregion Public 方法
}
