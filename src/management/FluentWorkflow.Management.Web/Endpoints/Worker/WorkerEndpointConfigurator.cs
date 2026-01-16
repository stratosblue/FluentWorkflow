using FluentWorkflow.Management.Web.EndpointLoader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.Worker;

internal class WorkerEndpointConfigurator : IExportEndpointConfigurator
{
    #region Public 属性

    public static string Prefix => "/worker";

    public static string Tag => "工作者";

    #endregion Public 属性

    #region Public 方法

    public static void Setup(WebApplication app, RouteGroupBuilder builder)
    {
        var loader = new StandardExportEndpointEndpointLoader(app, builder);

        loader.LoadStandard<GetDetailEndpoint>();
        loader.LoadStandard<GetMessagesEndpoint>();
        loader.LoadStandard<GetMessageDetailEndpoint>();
        loader.LoadStandard<ConsumptionControlEndpoint>();
    }

    #endregion Public 方法
}
