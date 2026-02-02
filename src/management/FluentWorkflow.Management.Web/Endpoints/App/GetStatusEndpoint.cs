using System.ComponentModel.DataAnnotations;
using FluentWorkflow.Management;
using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Worker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.App;

internal class GetStatusEndpoint : IStandardExportEndpoint<PagedResponseDto<WorkerStatusDto>>
{
    #region Public 属性

    /// <inheritdoc/>
    public static string Summary { get; } = "获取应用状态";

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public static RouteHandlerBuilder MapEndpoint(WebApplication app, RouteGroupBuilder builder)
    {
        return builder.MapGet("/status", (ManagementManagerHub managerHub, int page = 1, int pageSize = 10, [Required] string? appName = "default") =>
        {
            var manager = managerHub.GetManager(appName!);
            return manager.WorkerContexts.OrderBy(m => m.Value.Name)
                                         .Select(m => m.Value)
                                         .GetPagedResponse(page, pageSize, context =>
                                         {
                                             var descriptor = context.Descriptor;
                                             return new WorkerStatusDto(descriptor.Id, descriptor.Name, descriptor.ProcessingCount, context.IsActive);
                                         });
        });
    }

    #endregion Public 方法
}

/// <summary>
/// 工作者详情Dto
/// </summary>
/// <param name="Id"></param>
/// <param name="Name">工作者名称</param>
/// <param name="ProcessingCount">处理中数量</param>
/// <param name="IsActive">是否活跃状态</param>
public record WorkerStatusDto(Guid Id, string Name, int ProcessingCount, bool IsActive);
