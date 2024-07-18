using FluentWorkflow.Interface;
using Microsoft.Extensions.Hosting;

namespace FluentWorkflow.RabbitMQ;

internal sealed class BootstrapperHostedService : IHostedService
{
    #region Private 字段

    private readonly IFluentWorkflowBootstrapper _bootstrapper;

    #endregion Private 字段

    #region Public 构造函数

    public BootstrapperHostedService(IFluentWorkflowBootstrapper bootstrapper)
    {
        _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
    }

    #endregion Public 构造函数

    #region Public 方法

    public Task StartAsync(CancellationToken cancellationToken) => _bootstrapper.InitAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    #endregion Public 方法
}
