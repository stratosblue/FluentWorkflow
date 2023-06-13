using FluentWorkflow.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FluentWorkflow;

public class RabbitMQTestServiceProviderProvider : TestServiceProviderProvider
{
    #region Public 属性

    public static RabbitMQTestServiceProviderProvider Instance => new();

    #endregion Public 属性

    #region Private 字段

    private readonly Action<IServiceCollection>? _servicesSetup;

    #endregion Private 字段

    #region Public 构造函数

    public RabbitMQTestServiceProviderProvider(Action<IServiceCollection>? servicesSetup = null)
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
        connection.CreateModel().QueueDeleteNoWait(options.Value.ConsumeQueueName, false, false);
        await base.CleanupProviderAsync();
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<InMemoryWorkflowFinishWaiterContainer>();

        services.AddFluentWorkflow()
                .UseRabbitMQMessageDispatcher(options =>
                {
                    options.ConsumeQueueName = $"RabbitMQTestQueue-{DateTime.Now:yyyy:MM:dd:HH.mm.ss.ffff}";
                    options.Uri = new Uri(context.Configuration.GetRequiredSection("RabbitMQ").Value!);
                });

        _servicesSetup?.Invoke(services);
    }

    #endregion Protected 方法
}
