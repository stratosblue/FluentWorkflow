using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Worker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.App;

internal class GetAppsEndpoint : IStandardExportEndpoint<PagedResponseDto<AppsDetailDto>>
{
    #region Public 属性

    /// <inheritdoc/>
    public static string Summary { get; } = "获取应用列表";

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public static RouteHandlerBuilder MapEndpoint(WebApplication app, RouteGroupBuilder builder)
    {
        return builder.MapGet("/", (ManagementManagerHub managerHub, int page = 1, int pageSize = 20) =>
        {
            return managerHub.Managers.OrderBy(m => m.Key).GetPagedResponse(page, pageSize, m => new AppsDetailDto(m.Key, m.Value.WorkerContexts.Count));
        });
    }

    #endregion Public 方法
}

/// <summary>
/// 应用程序详情Dto
/// </summary>
/// <param name="Name">名称</param>
/// <param name="WorkerCount">工作者数量</param>
public record AppsDetailDto(string Name, int WorkerCount);
