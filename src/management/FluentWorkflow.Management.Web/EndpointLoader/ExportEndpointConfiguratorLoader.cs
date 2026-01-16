using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FluentWorkflow.Management.Web.EndpointLoader;

/// <summary>
/// 导出端点配置器加载器
/// </summary>
/// <param name="app"></param>
/// <param name="routePrefix"></param>
public sealed class ExportEndpointConfiguratorLoader(WebApplication app, string? routePrefix = null)
{
    #region Private 方法

    private string GetPrefix<TEndpointConfigurator>() where TEndpointConfigurator : IExportEndpointConfigurator
    {
        var prefix = string.IsNullOrEmpty(routePrefix)
             ? TEndpointConfigurator.Prefix
             : $"{routePrefix.TrimEnd('/')}/{TEndpointConfigurator.Prefix.TrimStart('/')}".TrimEnd('/');
        return prefix;
    }

    #endregion Private 方法

    #region Public 方法

    /// <summary>
    /// 加载导出端点配置器 <typeparamref name="TEndpointConfigurator"/>
    /// </summary>
    /// <typeparam name="TEndpointConfigurator"></typeparam>
    /// <returns></returns>
    public ExportEndpointConfiguratorLoader Load<TEndpointConfigurator>() where TEndpointConfigurator : IExportEndpointConfigurator
    {
        var builder = app.MapGroup(GetPrefix<TEndpointConfigurator>())
                         .WithTags(TEndpointConfigurator.Tag);

        TEndpointConfigurator.Setup(app, builder);

        return this;
    }

    #endregion Public 方法
}
