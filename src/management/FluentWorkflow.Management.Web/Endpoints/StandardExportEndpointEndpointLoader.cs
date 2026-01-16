using FluentWorkflow.Management.Web.EndpointLoader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints;

internal sealed class StandardExportEndpointEndpointLoader(WebApplication app, RouteGroupBuilder builder)
    : ExportEndpointLoader(app, builder)
{
    #region Public 方法

    public ExportEndpointLoader LoadStandard<TExportEndpoint>() where TExportEndpoint : IStandardExportEndpoint
    {
        var routeHandlerBuilder = InternalLoad<TExportEndpoint>();

        TExportEndpoint.ConfigureRouteHandlerBuilder(routeHandlerBuilder);

        return this;
    }

    #endregion Public 方法
}
