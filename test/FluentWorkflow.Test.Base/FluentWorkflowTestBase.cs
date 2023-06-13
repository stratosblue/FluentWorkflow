using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

public abstract class FluentWorkflowTestBase
{
    #region Protected 属性

    protected InMemoryWorkflowFinishWaiterContainer FinishWaiterContainer => TestServiceProviderProvider.ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>();

    protected IServiceProvider ServiceProvider => TestServiceProviderProvider.ServiceScope.ServiceProvider;

    protected TestServiceProviderProvider TestServiceProviderProvider { get; private set; } = null!;

    #endregion Protected 属性

    #region Public 方法

    [TestCleanup]
    public virtual async Task CleanupTestAsync()
    {
        await TestServiceProviderProvider.CleanupProviderAsync();
    }

    public abstract TestServiceProviderProvider GetProvider();

    [TestInitialize]
    public virtual async Task InitializeTest()
    {
        TestServiceProviderProvider = GetProvider();
        await TestServiceProviderProvider.InitializeProviderAsync(ConfigureServices);
    }

    #endregion Public 方法

    protected abstract void ConfigureServices(IServiceCollection services);
}
