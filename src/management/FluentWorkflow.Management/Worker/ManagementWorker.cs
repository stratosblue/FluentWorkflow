using System.ComponentModel.RuntimeValidation;
using System.Diagnostics;
using FluentWorkflow.Extensions;
using FluentWorkflow.Management.Shared;
using FluentWorkflow.Management.Shared.Communication;
using FluentWorkflow.Management.Shared.Features;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.Util;
using Hoarwell;
using Hoarwell.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentWorkflow.Management.Worker;

/// <summary>
/// 管理集群连接器
/// </summary>
public class ManagementClusterConnector : IAsyncDisposable
{
    #region Private 字段

    private readonly string _appName;

    private readonly ILogger _logger;

    private readonly ManagementClusterOptions _options;

    private readonly CancellationTokenSource _runningCTS = new();

    private readonly CancellationToken _runningToken;

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private AsyncServiceScope? _asyncServiceScope;

    private CancellationTokenRegistration? _cancellationTokenRegistration;

    private bool _hasConnected;

    private IHoarwellApplicationRunner? _hoarwellApplicationRunner;

    private IHoarwellContext? _hoarwellContext;

    private int _isDisposed;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 当前实例Id
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 连接器名称
    /// </summary>
    public string Name { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="ManagementClusterConnector"/>
    public ManagementClusterConnector([ServiceKey] string appName,
                                      IOptionsMonitor<ManagementClusterOptions> optionsMonitor,
                                      IServiceProvider serviceProvider,
                                      IServiceScopeFactory serviceScopeFactory,
                                      ILogger<ManagementClusterConnector> logger)
    {
        ArgumentException.ThrowIfNullOrEmpty(appName);
        ArgumentNullException.ThrowIfNull(optionsMonitor);

        _appName = appName;
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = optionsMonitor.Get(appName).Validation();
        _runningToken = _runningCTS.Token;

        Id = Guid.NewGuid();

        var workerStatistician = serviceProvider.GetRequiredService<WorkerStatistician>();

        _ = Task.Run(async () =>
        {
            //ping

            while (!_runningToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(45), _runningToken);

                try
                {
                    await PingAsync(_runningToken);
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Ping the server failed.");
                }
            }
        }, _runningToken);

        _ = Task.Run(async () =>
        {
            //上报处理中数量

            var processingCount = 0;

            while (!_runningToken.IsCancellationRequested)
            {
                try
                {
                    if (_hasConnected
                        && workerStatistician.ProcessingCount != processingCount)
                    {
                        processingCount = workerStatistician.ProcessingCount;
                        var report = new WorkerStatusReport()
                        {
                            ProcessingCount = processingCount,
                        };
                        await ReportAsync(report, _runningToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Report the status failed.");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), _runningToken);
            }
        }, _runningToken);

        Name = $"{FluentWorkflowEnvironment.Description}-{ObjectTagUtil.GetHashCodeTag(this)}";
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 进行连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed != 0, this);

        using var localCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _runningToken);

        cancellationToken = localCTS.Token;

        if (_hasConnected)
        {
            throw new InvalidOperationException("The instance has connected already");
        }

        await RunConnectLoopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
        {
            return;
        }

        _cancellationTokenRegistration?.Dispose();

        _semaphoreSlim.Dispose();

        try
        {
            _runningCTS.Cancel();
        }
        catch { }

        _runningCTS.Dispose();

        try
        {
            if (_hoarwellContext is { } context)
            {
                var close = new Close()
                {
                    Reason = "Client shutting down."
                };
                await context.RequiredCommunicationPipe()
                             .SendAsync(close, context.ExecutionAborted)
                             .WaitAsync(TimeSpan.FromSeconds(2));
            }
        }
        catch (Exception ex)
        {
            //忽略此处的异常
            Debug.WriteLine($"Client close notify failed: {ex}");
        }
        await _hoarwellContext.DisposeAsync();

        if (_hoarwellApplicationRunner is { } runner)
        {
            await runner.DisposeAsync();
        }

        if (_asyncServiceScope.HasValue)
        {
            var asyncServiceScope = _asyncServiceScope.Value;
            _asyncServiceScope = null;
            await asyncServiceScope.DisposeAsync();
        }
    }

    /// <summary>
    /// Ping连接是否正常
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task PingAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed != 0, this);
        await _hoarwellContext.Required().RequiredCommunicationPipe().PingAsync(cancellationToken);
    }

    /// <summary>
    /// 状态上报
    /// </summary>
    /// <param name="report"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ReportAsync(WorkerStatusReport report, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed != 0, this);
        await _hoarwellContext.Required().RequiredCommunicationPipe().SendAsync(report, cancellationToken);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"ClusterConnector: {Id} - {Name}";
    }

    #endregion Public 方法

    #region Private 方法

    private async Task InternalConnectAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed != 0, this);

        if (_hasConnected)
        {
            throw new InvalidOperationException("The instance has connected already");
        }

        if (_cancellationTokenRegistration is { } existedTokenRegistration)
        {
            existedTokenRegistration.Dispose();
        }

        if (_hoarwellContext is { } existedContext)
        {
            existedContext.Dispose();
        }

        if (_hoarwellApplicationRunner is { } existedApplicationRunner)
        {
            try
            {
                await existedApplicationRunner.DisposeAsync();
            }
            catch { }
        }

        if (_asyncServiceScope.HasValue)
        {
            var asyncServiceScope = _asyncServiceScope.Value;
            _asyncServiceScope = null;
            await asyncServiceScope.DisposeAsync();
        }

        var currentAsyncServiceScope = _serviceScopeFactory.CreateAsyncScope();

        var applicationRunner = currentAsyncServiceScope.ServiceProvider.GetRequiredKeyedService<IHoarwellApplicationRunner>(_appName);

        IHoarwellContext? context = null;
        try
        {
            using var contextWaiter = applicationRunner.GetContextWaiter();

            await applicationRunner.StartAsync(cancellationToken);

            context = await contextWaiter.Task.WaitAsync(cancellationToken);

            context.Features.Set(new IncreasingIdGenerator(1, 2));
            context.Features.Set(this);

            var communicationPipe = new CommunicationPipe(context);

            context.Features.Set<ICommunicationPipeFeature>(communicationPipe);
            context.Features.Set<IMessageAckFeature>(communicationPipe);

            var hello = new Hello()
            {
                Id = Id,
                ProtocolVersion = SharedConstants.ProtocolVersion,
                WhoIs = Name,
                HostName = Environment.MachineName,
                Cookie = _options.Cookie,
                StartupTime = Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                Version = SharedConstants.Version,
                Metadata = new Dictionary<string, string>(),
            };

            var welcome = await communicationPipe.RequestAsync<Hello, Welcome>(hello, cancellationToken);

            _hoarwellContext = context;
            _hoarwellApplicationRunner = applicationRunner;
            _asyncServiceScope = currentAsyncServiceScope;

            _cancellationTokenRegistration = context.ExecutionAborted.Register(() =>
            {
                _ = RunConnectLoopAsync(_runningToken);
            });

            _hasConnected = true;

            _logger.LogInformation("Connector {Name} has been connected.", this);
        }
        catch
        {
            //TODO 获取失败的原因
            await context.DisposeAsync();

            try
            {
                await applicationRunner.DisposeAsync();
            }
            catch { }

            await currentAsyncServiceScope.DisposeAsync();

            throw;
        }
    }

    private async Task RunConnectLoopAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        const int MaxRetryInterval = 10 * 60;
        var retryInterval = 10;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken);

                try
                {
                    _hasConnected = false;

                    var isRecovery = _asyncServiceScope.HasValue;

                    await InternalConnectAsync(cancellationToken);

                    if (isRecovery)
                    {
                        _logger.LogInformation("Connector {Name}'s connection recovery success.", this);
                    }
                    return;
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
            catch (Exception ex)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogWarning(ex, "Error at connector {Name} reconnect to server. Retry after {Seconds} seconds.", this, retryInterval);

                await Task.Delay(TimeSpan.FromSeconds(retryInterval), cancellationToken);

                if (retryInterval < MaxRetryInterval)
                {
                    retryInterval *= 2;
                    if (retryInterval > MaxRetryInterval)
                    {
                        retryInterval = MaxRetryInterval;
                    }
                }
            }
        }
    }

    #endregion Private 方法
}
