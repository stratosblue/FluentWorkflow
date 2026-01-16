#pragma warning disable IDE0130

using System.ComponentModel;
using FluentWorkflow.Management.Web.UI;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// ManagementUI build extensions
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ManagementUIBuildExtensions
{
    #region Public 方法

    /// <summary>
    /// 映射FluentWorkflow管理端UI
    /// </summary>
    /// <param name="app"></param>
    /// <param name="optionsSetup"></param>
    /// <returns></returns>
    public static IApplicationBuilder MapFluentWorkflowManagementUI(this IApplicationBuilder app, Action<ManagementUIOptions> optionsSetup)
    {
        var options = new ManagementUIOptions();
        optionsSetup(options);

        var routePrefix = string.IsNullOrWhiteSpace(options.RoutePrefix) ? ManagementUIOptions.DefaultRoutePrefix : options.RoutePrefix;

        app.Map(routePrefix, managementApp =>
        {
            managementApp.UseMiddleware<ManagementUIMiddleware>(options);
        });

        return app;
    }

    /// <summary>
    /// 映射FluentWorkflow管理端UI
    /// </summary>
    /// <param name="app"></param>
    /// <param name="routePrefix">入口点路径</param>
    /// <param name="apiEndpoint">api端点路径</param>
    /// <returns></returns>
    public static IApplicationBuilder MapFluentWorkflowManagementUI(this IApplicationBuilder app,
                                                                    string routePrefix = ManagementUIOptions.DefaultRoutePrefix,
                                                                    string apiEndpoint = ManagementUIOptions.DefaultApiEndpoint)
    {
        return app.MapFluentWorkflowManagementUI(options =>
        {
            options.RoutePrefix = routePrefix;
            options.ApiEndpoint = apiEndpoint;
        });
    }

    #endregion Public 方法
}
