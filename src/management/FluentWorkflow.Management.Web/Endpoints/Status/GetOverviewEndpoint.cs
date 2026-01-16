using FluentWorkflow.Management;
using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Worker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.Status;

internal class GetOverviewEndpoint : IStandardExportEndpoint<OverviewDto>
{
    #region Public 属性

    /// <inheritdoc/>
    public static string Summary { get; } = "概览";

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public static RouteHandlerBuilder MapEndpoint(WebApplication app, RouteGroupBuilder builder)
    {
        return builder.MapGet("/overview", (ManagementManagerHub managerHub) =>
        {
            var result = new OverviewDto(managerHub.AppNames);
            return result;
        });
    }

    #endregion Public 方法
}

/// <summary>
/// 概览Dto
/// </summary>
/// <param name="Applications">应用程序名称列表</param>
public record OverviewDto(ICollection<string> Applications);
