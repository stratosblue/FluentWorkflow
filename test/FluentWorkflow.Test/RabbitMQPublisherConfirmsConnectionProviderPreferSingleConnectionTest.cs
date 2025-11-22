using FluentWorkflow.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentWorkflow;

[TestClass]
public class RabbitMQPublisherConfirmsConnectionProviderPreferSingleConnectionTest : TestServiceProviderProvider
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
                    var testId = $"{Environment.Version.Major}_{Environment.Version.Minor}-{DateTime.Now:HH.mm.ss.FFFFFFF}@{Guid.NewGuid().ToString()[..6]}";

                    options.ExchangeName = $"fwf-test-{testId}";
                    options.ConsumeQueueName = $"FWFTestQueue-{testId}";
                    options.Uri = new Uri(context.Configuration.GetRequiredSection("RabbitMQ").Value!);
                    options.PreferSingleConnection = true;
                    options.PublisherConfirms = true;

#pragma warning disable CS0618 // 类型或成员已过时
                    //测试时不持久化，提升一点测试速度
                    options.Durable = false;
#pragma warning restore CS0618 // 类型或成员已过时
                });
    }

    #endregion Protected 方法
}
