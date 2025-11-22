using FluentWorkflow.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentWorkflow;

[TestClass]
public class RabbitMQConnectionProviderPreferSingleConnectionTest : TestServiceProviderProvider
{
    #region Public 方法

    [TestMethod]
    public async Task Should_Be_Same_Instance()
    {
        var connectionProvider = ServiceProvider.GetRequiredService<IRabbitMQConnectionProvider>();
        var tasks = Enumerable.Range(0, 20).Select(_ => connectionProvider.GetAsync(default)).ToList();
        await Task.WhenAll(tasks);

        var instances = tasks.Select(m => m.Result).ToList();
        var count = instances.Distinct().Count();
        Assert.AreEqual(1, count);
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddFluentWorkflow()
                .UseRabbitMQMessageDispatcher(options =>
                {
                    options.ExchangeName = $"fwf-test-exchange-{Environment.Version.Major}_{Environment.Version.Minor}";
                    options.ConsumeQueueName = $"FWFTestQueue-{DateTime.Now:yyyy:MM:dd:HH.mm.ss.ffff}";
                    options.Uri = new Uri(context.Configuration.GetRequiredSection("RabbitMQ").Value!);
                    options.PreferSingleConnection = true;
                    options.PublisherConfirms = false;
                });
    }

    #endregion Protected 方法
}
