using FluentWorkflow.Management.Web.EndpointLoader;
using FluentWorkflow.Management.Web.Endpoints.App;
using FluentWorkflow.Management.Web.Endpoints.Status;
using FluentWorkflow.Management.Web.Endpoints.Worker;
using FluentWorkflow.Management.Web.UI;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// FluentWorkflow管理端webapp拓展
/// </summary>
public static class FluentWorkflowManagementWebAppExtensions
{
    #region Public 方法

    /// <summary>
    /// 映射管理端API端点
    /// </summary>
    /// <param name="app"></param>
    /// <param name="routePrefix"></param>
    public static void MapFluentWorkflowManagementApi(this WebApplication app, string routePrefix = ManagementUIOptions.DefaultApiEndpoint)
    {
        var loader = new ExportEndpointConfiguratorLoader(app, routePrefix);

        loader.Load<StatusEndpointConfigurator>();
        loader.Load<AppEndpointConfigurator>();
        loader.Load<WorkerEndpointConfigurator>();
    }

    #endregion Public 方法
}
