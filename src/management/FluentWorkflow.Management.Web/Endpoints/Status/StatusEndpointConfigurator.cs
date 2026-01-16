using FluentWorkflow.Management.Web.EndpointLoader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.Status;

internal class StatusEndpointConfigurator : IExportEndpointConfigurator
{
    #region Public 属性

    public static string Prefix => "/status";

    public static string Tag => "状态";

    #endregion Public 属性

    #region Public 方法

    public static void Setup(WebApplication app, RouteGroupBuilder builder)
    {
        var loader = new StandardExportEndpointEndpointLoader(app, builder);

        loader.LoadStandard<GetOverviewEndpoint>();
    }

    #endregion Public 方法
}
