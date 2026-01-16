using FluentWorkflow.Management.Web.EndpointLoader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FluentWorkflow.Management.Web.Endpoints;

/// <summary>
/// 标准导出端点
/// </summary>
public interface IStandardExportEndpoint : IExportEndpoint
{
    #region Public 方法

    /// <summary>
    /// 配置路由构建器
    /// </summary>
    /// <param name="routeHandlerBuilder"></param>
    public static abstract void ConfigureRouteHandlerBuilder(RouteHandlerBuilder routeHandlerBuilder);

    #endregion Public 方法
}

/// <summary>
/// 标准导出端点
/// </summary>
/// <typeparam name="TResponse">端点响应的数据类型</typeparam>
public interface IStandardExportEndpoint<TResponse> : IStandardExportEndpoint
{
    #region Public 方法

    /// <inheritdoc/>
    static void IStandardExportEndpoint.ConfigureRouteHandlerBuilder(RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder.AddEndpointFilter<StandardEndpointFilter<TResponse>>();

        routeHandlerBuilder.Add(static m =>
        {
            foreach (var item in m.Metadata.OfType<ProducesResponseTypeMetadata>().ToList())
            {
                m.Metadata.Remove(item);
            }
        });

        routeHandlerBuilder.Produces<StandardApiResponse<TResponse>>();
    }

    #endregion Public 方法
}
