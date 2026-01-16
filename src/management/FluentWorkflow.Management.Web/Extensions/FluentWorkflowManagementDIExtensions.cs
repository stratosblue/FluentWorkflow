using FluentWorkflow.Management.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// FluentWorkflow管理功能Di拓展
/// </summary>
public static class FluentWorkflowManagementDIExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加管理端API
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFluentWorkflowManagementApi(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializerContext.Default);
        });
        return services;
    }

    #endregion Public 方法
}
