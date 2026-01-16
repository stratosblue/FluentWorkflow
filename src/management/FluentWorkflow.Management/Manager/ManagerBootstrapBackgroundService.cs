using FluentWorkflow.Management.Shared;
using Hoarwell;
using Hoarwell.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentWorkflow.Management.Manager;

internal class ManagerBootstrapBackgroundService(IOptions<ManagementGlobalOptions> options,
                                                 ILogger<ManagerBootstrapBackgroundService> logger,
                                                 IServiceProvider serviceProvider)
    : BackgroundService
{
    #region Protected 方法

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var names = options.Value.RegisteredAppNames;

        var runners = names.Select(name => (Name: name, Runner: serviceProvider.GetRequiredKeyedService<IHoarwellApplicationRunner>(name)))
                           .ToList();

        var startTasks = runners.Select(async runnerItem =>
        {
            var (name, runner) = runnerItem;

            try
            {
                logger.LogInformation("The management server for {App} is currently starting.", name);
                await runner.StartAsync(stoppingToken);
                var endPoints = string.Join(", ", runner.Features.Get<ILocalEndPointsFeature>()?.EndPoints?.Select(m => m.ToString()) ?? []);
                logger.LogInformation("The management server for {App} has started. Now serve at {Endpoint}.", name, endPoints);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "The management server for {App} startup failed.", name);
                throw;
            }
        }).ToList();

        await Task.WhenAll(startTasks).WaitAsync(stoppingToken);
    }

    #endregion Protected 方法
}
