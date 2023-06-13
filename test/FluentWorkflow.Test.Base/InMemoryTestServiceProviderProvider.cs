using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentWorkflow;

public class InMemoryTestServiceProviderProvider : TestServiceProviderProvider
{
    #region Public 属性

    public static InMemoryTestServiceProviderProvider Instance => new();

    #endregion Public 属性

    #region Private 字段

    private readonly Action<IServiceCollection>? _servicesSetup;

    #endregion Private 字段

    #region Public 构造函数

    public InMemoryTestServiceProviderProvider(Action<IServiceCollection>? servicesSetup = null)
    {
        _servicesSetup = servicesSetup;
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<InMemoryWorkflowFinishWaiterContainer>();

        services.AddFluentWorkflow()
                .UseInMemoryWorkflowMessageDispatcher();

        _servicesSetup?.Invoke(services);
    }

    #endregion Protected 方法
}
