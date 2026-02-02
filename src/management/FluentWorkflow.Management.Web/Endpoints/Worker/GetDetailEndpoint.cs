using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.Management.Worker;
using Hoarwell.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow.Management.Web.Endpoints.Worker;

internal class GetDetailEndpoint : IStandardExportEndpoint<WorkerDetailDto>
{
    #region Public 属性

    /// <inheritdoc/>
    public static string Summary { get; } = "获取工作者详情";

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public static RouteHandlerBuilder MapEndpoint(WebApplication app, RouteGroupBuilder builder)
    {
        return builder.MapGet("/", static async (string appName,
                                          Guid workerId,
                                          ManagementManagerHub managerHub,
                                          ILogger<GetDetailEndpoint> logger,
                                          CancellationToken cancellationToken = default) =>
        {
            var context = managerHub.GetWorker(appName, workerId);
            var descriptor = context.Descriptor;
            var inboundOutboundIdleStateFeature = context.Context.Features.Get<IInboundOutboundIdleStateFeature>();
            DateTime? lastActive = null;
            if (inboundOutboundIdleStateFeature is not null)
            {
                lastActive = inboundOutboundIdleStateFeature.GetLastActiveTime();
            }

            WorkerStatistics? workerStatistics = null;

            if (context.IsActive)
            {
                try
                {
                    var request = new WorkerStatisticsRequest();

                    workerStatistics = await context.CommunicationPipe.RequestAsync<WorkerStatisticsRequest, WorkerStatistics>(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error at get worker statistics.");
                }
            }

            return new WorkerDetailDto(descriptor.Id,
                                       context.Name,
                                       descriptor.HostName,
                                       descriptor.Version,
                                       descriptor.StartupTime,
                                       descriptor.Metadata,
                                       descriptor.RemoteEndPoint?.ToString(),
                                       context.IsActive,
                                       workerStatistics?.ProcessingCount ?? 0,
                                       workerStatistics?.MessageStatistics.Select(static m => new WorkerMessageTimeSequenceStatisticsDto(m.TimeSpan, m.IncomingCount, m.CompletedCount, m.AverageProcessingTimeSpan)).ToList() ?? [],
                                       lastActive);
        });
    }

    #endregion Public 方法
}

/// <summary>
/// 工作者详情Dto
/// </summary>
/// <param name="Id"></param>
/// <param name="Name">工作者名称</param>
/// <param name="HostName">主机名称</param>
/// <param name="Version">版本号</param>
/// <param name="StartupTime">启动事件</param>
/// <param name="Metadata">元数据</param>
/// <param name="RemoteEndPoint">远程端点</param>
/// <param name="IsActive">是否活跃状态</param>
/// <param name="ProcessingCount">处理中的消息数量</param>
/// <param name="MessageStatistics">消息统计</param>
/// <param name="LastActive">最后活跃时间</param>
public record WorkerDetailDto(Guid Id,
                              string Name,
                              string HostName,
                              string Version,
                              DateTime StartupTime,
                              IDictionary<string, string> Metadata,
                              string? RemoteEndPoint,
                              bool IsActive,
                              int ProcessingCount,
                              IList<WorkerMessageTimeSequenceStatisticsDto> MessageStatistics,
                              DateTime? LastActive)
{
}

/// <summary>
/// 工作者消息时间跨度统计Dto
/// </summary>
/// <param name="TimeSpan">时间跨度</param>
/// <param name="IncomingCount">传入的消息数量</param>
/// <param name="CompletedCount">完成的消息数量</param>
/// <param name="AverageProcessingTimeSpan">平均处理时长</param>
public record WorkerMessageTimeSequenceStatisticsDto(TimeSpan TimeSpan, int IncomingCount, int CompletedCount, TimeSpan AverageProcessingTimeSpan)
{
}
