using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.EndpointLoader;

/// <summary>
/// 导出端点加载器
/// </summary>
/// <param name="app"></param>
/// <param name="builder"></param>
public class ExportEndpointLoader(WebApplication app, RouteGroupBuilder builder)
{
    #region Public 方法

    /// <summary>
    /// 加载导出端点<typeparamref name="TExportEndpoint"/>
    /// </summary>
    /// <typeparam name="TExportEndpoint"></typeparam>
    /// <returns></returns>
    public virtual ExportEndpointLoader Load<TExportEndpoint>() where TExportEndpoint : IExportEndpoint
    {
        InternalLoad<TExportEndpoint>();
        return this;
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 加载导出端点<typeparamref name="TExportEndpoint"/>
    /// </summary>
    /// <typeparam name="TExportEndpoint"></typeparam>
    /// <returns></returns>
    protected RouteHandlerBuilder InternalLoad<TExportEndpoint>() where TExportEndpoint : IExportEndpoint
    {
        var handlerBuilder = TExportEndpoint.MapEndpoint(app, builder);

        handlerBuilder.WithSummary(TExportEndpoint.Summary);

        return handlerBuilder;
    }

    #endregion Protected 方法
}
