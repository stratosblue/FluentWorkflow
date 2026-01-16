using System.ComponentModel.RuntimeValidation;
using FluentWorkflow.Management.Shared;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FluentWorkflow.Management.Worker;

internal class ClusterConnectorBootstrapBackgroundService(IOptions<ManagementGlobalOptions> options,
                                                          WorkerStatistician workerStatistician,
                                                          IServiceProvider serviceProvider)
    : BackgroundService
{
    #region Protected 方法

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //加载以初始化
        workerStatistician.Required();

        var names = options.Value.RegisteredWorkerNames;

        await using var serviceScope = serviceProvider.CreateAsyncScope();

        var connectors = names.Select(name => serviceScope.ServiceProvider.GetRequiredKeyedService<ManagementClusterConnector>(name))
                              .ToList();

        foreach (var connector in connectors)
        {
            _ = Task.Run(() => connector.ConnectAsync(stoppingToken), stoppingToken);
        }

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    #endregion Protected 方法
}
