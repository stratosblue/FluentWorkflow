using Microsoft.Extensions.Hosting;

namespace FluentWorkflow.RabbitMQ;

internal sealed class BootstrapperHostedService(IFluentWorkflowBootstrapper bootstrapper)
    : IHostedService
{
    #region Public 方法

    public Task StartAsync(CancellationToken cancellationToken) => bootstrapper.InitAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    #endregion Public 方法
}
