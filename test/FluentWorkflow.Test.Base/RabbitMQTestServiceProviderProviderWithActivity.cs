using System.Collections.Concurrent;
using System.Diagnostics;
using FluentWorkflow.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FluentWorkflow;

public class RabbitMQTestServiceProviderProviderWithActivity : TestServiceProviderProvider
{
    #region Public 属性

    public static RabbitMQTestServiceProviderProviderWithActivity Instance => new();

    public ConcurrentQueue<ActivityInfo> ActivityQueue { get; } = new();

    #endregion Public 属性

    #region Private 字段

    private readonly Action<IServiceCollection>? _servicesSetup;

    private ActivityListener? _activityListener;

    #endregion Private 字段

    #region Public 构造函数

    public RabbitMQTestServiceProviderProviderWithActivity(Action<IServiceCollection>? servicesSetup = null)
    {
        _servicesSetup = servicesSetup;
    }

    #endregion Public 构造函数

    #region Protected 方法

    public override async Task CleanupProviderAsync()
    {
        var options = ServiceProvider.GetRequiredService<IOptions<RabbitMQOptions>>();
        var connectionProvider = ServiceProvider.GetRequiredService<IRabbitMQConnectionProvider>();
        using var connection = await connectionProvider.GetAsync(default);
#if LEGACY_RABBITMQ
        using var model = connection.CreateModel();
        model.QueueDeleteNoWait(options.Value.ConsumeQueueName, false, false);
#else
        using var model = await connection.CreateChannelAsync();
        await model.QueueDeleteAsync(options.Value.ConsumeQueueName!, false, false);
#endif
        await base.CleanupProviderAsync();
        _activityListener?.Dispose();
        _activityListener = null;
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        _activityListener = new ActivityListener()
        {
            ShouldListenTo = source => source.Name.StartsWith(Diagnostics.DiagnosticConstants.ActivityNames.RootActivitySourceName),
            ActivityStarted = activity => ActivityQueue.Enqueue(new(activity, true)),
            ActivityStopped = activity => ActivityQueue.Enqueue(new(activity, false)),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(_activityListener);

        services.AddSingleton<InMemoryWorkflowFinishWaiterContainer>();

        services.AddFluentWorkflow()
                .UseRabbitMQMessageDispatcher(options =>
                {
                    options.ExchangeName = $"fwf-test-exchange-{Environment.Version.Major}_{Environment.Version.Minor}";
                    options.ConsumeQueueName = $"RabbitMQTestQueue-{DateTime.Now:yyyy:MM:dd:HH.mm.ss.ffff}";
                    options.Uri = new Uri(context.Configuration.GetRequiredSection("RabbitMQ").Value!);
                    options.PublisherConfirms = false;
                });

        _servicesSetup?.Invoke(services);
    }

    #endregion Protected 方法

    public record struct ActivityInfo(Activity Activity, bool IsStart);
}
