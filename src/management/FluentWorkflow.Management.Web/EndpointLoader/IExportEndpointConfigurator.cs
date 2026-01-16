using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.EndpointLoader;

/// <summary>
/// 导出端点配置器
/// </summary>
public interface IExportEndpointConfigurator
{
    #region Public 属性

    /// <summary>
    /// 端点前缀
    /// </summary>
    public static abstract string Prefix { get; }

    /// <summary>
    /// 端点tag
    /// </summary>
    public static abstract string Tag { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 装配端点
    /// </summary>
    /// <param name="app"></param>
    /// <param name="builder"></param>
    public static abstract void Setup(WebApplication app, RouteGroupBuilder builder);

    #endregion Public 方法
}
