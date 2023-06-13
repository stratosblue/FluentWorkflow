using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow;

public abstract class TestServiceProviderProvider
{
    #region Protected 属性

    public InMemoryWorkflowFinishWaiterContainer FinishWaiterContainer => ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>();

    public IServiceProvider RootServiceProvider { get; private set; } = null!;

    public IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

    public IServiceScope ServiceScope { get; private set; } = null!;

    public IHost TestHost { get; private set; } = null!;

    #endregion Protected 属性

    #region Public 方法

    public virtual async Task CleanupProviderAsync()
    {
        ServiceScope.Dispose();
        await TestHost.StopAsync();
        TestHost.Dispose();
    }

    public virtual async Task InitializeProviderAsync(Action<IServiceCollection>? configureServices)
    {
        var builder = Host.CreateDefaultBuilder()
                          .ConfigureAppConfiguration(builder =>
                          {
                              builder.AddUserSecrets("FluentWorkflowTest_05b88fa0-f476-470f-a4f6-9c5a087b5eb7");
                          })
                          .ConfigureLogging(builder =>
                          {
                              builder.ClearProviders();
                              builder.AddSimpleConsole(options =>
                              {
                                  options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
                                  options.SingleLine = false;
                              });
                              builder.SetMinimumLevel(LogLevel.Trace);
                          })
                          .ConfigureServices((context, services) =>
                          {
                              ConfigureServices(context, services);
                              configureServices?.Invoke(services);
                          });

        TestHost = builder.Build();
        await TestHost.StartAsync();

        RootServiceProvider = TestHost.Services;
        ServiceScope = RootServiceProvider.CreateScope();
    }

    #endregion Public 方法

    #region Protected 方法

    protected abstract void ConfigureServices(HostBuilderContext context, IServiceCollection services);

    #endregion Protected 方法
}
