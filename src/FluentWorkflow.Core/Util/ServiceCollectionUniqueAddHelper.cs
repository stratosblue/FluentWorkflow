using System.ComponentModel;
using FluentWorkflow.Interface;
using FluentWorkflow.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FluentWorkflow;

/// <summary>
/// <see cref="IServiceCollection"/> 唯一添加帮助类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ServiceCollectionUniqueAddHelper
{
    #region Public 方法

    /// <summary>
    /// 注册<see cref="IWorkflowContinuator"/>
    /// </summary>
    /// <typeparam name="TWorkflowContinuator"></typeparam>
    /// <param name="services"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    public static bool RegisterContinuator<TWorkflowContinuator>(IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TWorkflowContinuator : IWorkflowContinuator, IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
    {
        services.TryAdd(ServiceDescriptor.Describe(typeof(TWorkflowContinuator), typeof(TWorkflowContinuator), serviceLifetime));

        return UniqueAdd<IPostConfigureOptions<FluentWorkflowOptions>, FluentWorkflowOptionsContinuatorRegister<TWorkflowContinuator>>(services, ServiceLifetime.Singleton);
    }

    /// <summary>
    /// 确保 <paramref name="services"/> 只有唯一一条数据
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    public static bool UniqueAdd<TService, TImplementation>(IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        int count = services.Count;
        for (int i = 0; i < count; i++)
        {
            var item = services[i];
            if (item.ServiceType == typeof(TService)
                && item.ImplementationType == typeof(TImplementation))
            {
                return false;
            }
        }
        services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), serviceLifetime));

        return true;
    }

    #endregion Public 方法
}
