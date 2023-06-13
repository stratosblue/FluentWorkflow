using DotNetCore.CAP;
using FluentWorkflow.GenericExtension.FluentWorkflowSimpleSample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Savorboard.CAP.InMemoryMessageQueue;

namespace FluentWorkflow;

public class CAPTestServiceProviderProvider : TestServiceProviderProvider
{
    #region Public 属性

    public static CAPTestServiceProviderProvider Instance => new();

    #endregion Public 属性

    #region Private 字段

    private readonly Action<IServiceCollection>? _servicesSetup;

    #endregion Private 字段

    #region Public 构造函数

    public CAPTestServiceProviderProvider(Action<IServiceCollection>? servicesSetup = null)
    {
        _servicesSetup = servicesSetup;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override async Task InitializeProviderAsync(Action<IServiceCollection>? configureServices)
    {
        await base.InitializeProviderAsync(configureServices);

        await TestHost.Services.GetRequiredService<IBootstrapper>().BootstrapAsync(default);
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddCap(x =>
        {
            x.UseInMemoryStorage();
            x.UseInMemoryMessageQueue();
        });

        //移除CAP初始化的 HostedService ，后续手动初始化
        services.RemoveAll<IHostedService>();

        services.AddSingleton<InMemoryWorkflowFinishWaiterContainer>();

        services.AddFluentWorkflow().UseCapPublisherWorkflowMessageDispatcher();

        _servicesSetup?.Invoke(services);
    }

    #endregion Protected 方法
}
