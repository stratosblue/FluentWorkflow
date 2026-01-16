using FluentWorkflow.Management.Web.EndpointLoader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.App;

internal class AppEndpointConfigurator : IExportEndpointConfigurator
{
    #region Public 属性

    /// <inheritdoc/>
    public static string Prefix => "/app";

    /// <inheritdoc/>
    public static string Tag => "应用";

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public static void Setup(WebApplication app, RouteGroupBuilder builder)
    {
        var loader = new StandardExportEndpointEndpointLoader(app, builder);

        loader.LoadStandard<GetAppsEndpoint>();
        loader.LoadStandard<GetStatusEndpoint>();
    }

    #endregion Public 方法
}
