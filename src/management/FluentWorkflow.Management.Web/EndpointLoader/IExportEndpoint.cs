using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.EndpointLoader;

/// <summary>
/// 导出端点接口
/// </summary>
public interface IExportEndpoint
{
    #region Public 属性

    /// <summary>
    /// 端点摘要
    /// </summary>
    public static abstract string Summary { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 配置端点映射
    /// </summary>
    /// <param name="app"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static abstract RouteHandlerBuilder MapEndpoint(WebApplication app, RouteGroupBuilder builder);

    #endregion Public 方法
}
