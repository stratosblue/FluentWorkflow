using System.Collections.Immutable;
using System.ComponentModel.RuntimeValidation;
using FluentWorkflow.Management.Shared;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentWorkflow.Management.Manager;

/// <summary>
///管理器中心
/// </summary>
public class ManagementManagerHub : IDisposable
{
    #region Private 字段

    private readonly ILogger _logger;

    private readonly CancellationTokenSource _runningCTS = new();

    private readonly CancellationToken _runningToken;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 应用程序名称列表
    /// </summary>
    public ImmutableHashSet<string> AppNames { get; }

    /// <summary>
    /// 管理器集合
    /// </summary>
    public ImmutableDictionary<string, ManagementManager> Managers { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="ManagementManagerHub"/>
    public ManagementManagerHub(IOptionsMonitor<ManagementGlobalOptions> optionsMonitor,
                                ILogger<ManagementManager> logger,
                                IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _logger = logger;
        _runningToken = _runningCTS.Token;

        var registeredAppNames = optionsMonitor.CurrentValue.RegisteredAppNames;

        AppNames = registeredAppNames.ToImmutableHashSet();
        Managers = registeredAppNames.ToImmutableDictionary(m => m, m => serviceProvider.GetRequiredKeyedService<ManagementManager>(m));

        _ = Task.Run(async () =>
        {
            var cancellationToken = _runningToken;
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(optionsMonitor.CurrentValue.ManagerRefreshInterval, cancellationToken);

                foreach (var (name, manager) in Managers)
                {
                    try
                    {
                        _logger.LogDebug("Start refresh manager {Name}.", name);
                        await manager.RefreshAsync(cancellationToken);
                        _logger.LogDebug("Manager {Name} refreshed.", name);
                    }
                    catch (Exception ex)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        _logger.LogError(ex, "Error at refresh manager {Name}.", name);
                    }
                }
            }
        }, _runningToken);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
        //不在此处处理Managers，其由DI容器进行处理

        try
        {
            _runningCTS.Cancel();
        }
        catch { }

        _runningCTS.Dispose();
    }

    /// <summary>
    /// 获取应用程序 <paramref name="appName"/> 的管理器
    /// </summary>
    /// <param name="appName"></param>
    /// <returns></returns>
    /// <exception cref="ResourceNotFoundException"></exception>
    public ManagementManager GetManager(string appName)
    {
        appName.Required();
        if (Managers.TryGetValue(appName, out var manager))
        {
            return manager;
        }
        throw new ResourceNotFoundException("app", appName);
    }

    /// <summary>
    /// 获取应用程序 <paramref name="appName"/> 的工作者 <paramref name="workerId"/>
    /// </summary>
    /// <param name="appName"></param>
    /// <param name="workerId"></param>
    /// <returns></returns>
    /// <exception cref="ResourceNotFoundException"></exception>
    public WorkerContext GetWorker(string appName, Guid workerId)
    {
        if (GetManager(appName).WorkerContexts.TryGetValue(workerId, out var workerContext))
        {
            return workerContext;
        }
        throw new ResourceNotFoundException("worker", workerId.ToString("N"));
    }

    #endregion Public 方法
}
