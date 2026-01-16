#pragma warning disable IDE0130

using System.Net;
using FluentWorkflow.Management;
using FluentWorkflow.Management.Shared;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.Management.Worker;
using FluentWorkflow.Management.Worker.MessageHandler;
using FluentWorkflow.MessageDispatch.DispatchControl;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// WorkerDI拓展
/// </summary>
public static class FluentWorkflowWorkerDIExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加FluentWorkflow管理功能的Worker支持
    /// </summary>
    /// <param name="services"></param>
    /// <param name="endPoint">管理端连接端点</param>
    /// <param name="cookie">用于连接的Cookie(需要统一)</param>
    /// <param name="appName">应用程序名称</param>
    /// <returns></returns>
    public static IServiceCollection AddFluentWorkflowManagementWorker(this IServiceCollection services, EndPoint endPoint, string cookie, string appName = "default")
    {
        services.AddHostedService<ClusterConnectorBootstrapBackgroundService>();
        services.TryAddSingleton<WorkerStatistician>();
        services.TryAddSingleton<IWorkingController, DefaultWorkingController>();

        services.AddOptions<ManagementGlobalOptions>()
                .Configure(options =>
                {
                    options.RegisteredWorkerNames.Add(appName);
                });

        services.TryAddKeyedScoped<ManagementClusterConnector>(appName);
        services.AddOptions<ManagementClusterOptions>(appName)
                .Configure(options =>
                {
                    options.Cookie = cookie;
                });

        services.AddHoarwell(appName)
                .UseDefaultTransientStreamApplication()
                .UseDefaultSocketTransportClient(options => options.EndPoints.Add(endPoint), ServiceLifetime.Transient)
                .ConfigureMessageSerialization()
                .ConfigureInboundPipeline(pipelineBuilder =>
                {
                    pipelineBuilder.UsePipeReaderAdaptMiddleware()
                                   .UseUInt32LengthFieldBasedFrameDecoder()
                                   .UseDefaultMessageDeserializer()
                                   .RunEndpoint(endpointBuilder =>
                                   {
                                       //TODO 在此处配置更多的需要处理的消息
                                       endpointBuilder.GenericAck<Welcome>();
                                       endpointBuilder.GenericAck<Pong>();
                                       endpointBuilder.HandleMessage<Ping, NoAuthorizePingPongMessageHandler>();
                                       endpointBuilder.HandleMessage<ConsumptionControl, ConsumptionControlMessageHandler>();
                                       endpointBuilder.HandleMessage<MessageListQueryRequest, MessageListQueryMessageHandler>();
                                       endpointBuilder.HandleMessage<MessageQueryRequest, MessageQueryMessageHandler>();
                                       endpointBuilder.HandleMessage<WorkerStatisticsRequest, WorkerStatisticsMessageHandler>();

                                       endpointBuilder.OnMessage<Close>((context, message) =>
                                       {
                                           context.Services.GetService<ILogger<ManagementClusterConnector>>()
                                                           ?.LogWarning("Connection closing by server: {Reason}", message.Data.Reason);
                                           return Task.CompletedTask;
                                       });
                                   });
                })
                .ConfigureOutboundPipeline(pipelineBuilder =>
                {
                    pipelineBuilder.UseUInt32LengthFieldBasedFrameEncoder()
                                   .RunDefaultMessageSerializer();
                });

        return services;
    }

    #endregion Public 方法
}
