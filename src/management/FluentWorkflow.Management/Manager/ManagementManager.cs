using System.Collections.Concurrent;
using FluentWorkflow.Management.Shared;
using Hoarwell;
using Hoarwell.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow.Management.Manager;

/// <summary>
/// 管理器
/// </summary>
/// <param name="appName"></param>
/// <param name="logger"></param>
public class ManagementManager([ServiceKey] string appName,
                               ILogger<ManagementManager> logger)
    : IAsyncDisposable
{
    #region Private 字段

    private readonly CancellationTokenSource _runningCTS = new();

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 工作者上下文集合
    /// </summary>
    public ConcurrentDictionary<Guid, WorkerContext> WorkerContexts { get; set; } = new();

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        try
        {
            _runningCTS.Cancel();
        }
        catch { }
        finally
        {
            _runningCTS.Dispose();
        }
        _semaphoreSlim.Dispose();

        foreach (var (_, value) in WorkerContexts)
        {
            value.Context.Abort("Server shuttingdown");
        }

        WorkerContexts.Clear();
    }

    /// <summary>
    /// 刷新管理器信息
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        //直接锁，方便快捷
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            foreach (var name in WorkerContexts.Keys)
            {
                try
                {
                    if (WorkerContexts.TryGetValue(name, out var context)
                        && !context.IsActive)
                    {
                        //TODO 时间可配置
                        var lastActiveTime = context.Context.RequiredFeature<IInboundOutboundIdleStateFeature>().GetLastActiveTime();
                        if (!lastActiveTime.HasValue
                            || lastActiveTime.Value.AddMinutes(-10) < DateTime.UtcNow)
                        {
                            WorkerContexts.TryRemove(name, out _);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogInformation(ex, "Error at refresh worker {Name} in {App}.", name, appName);
                }
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// 将工作者连接到管理器
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task WorkerConnectedAsync(WorkerContext context)
    {
        var localCts = CancellationTokenSource.CreateLinkedTokenSource(_runningCTS.Token, context.CancellationToken);
        var cancellationToken = localCts.Token;
        var id = context.Id;

        //直接锁，方便快捷
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            if (WorkerContexts.TryGetValue(id, out var existedContext))
            {
                var existedContextPingable = false;
                try
                {
                    existedContextPingable = await existedContext.CommunicationPipe.PingAsync(cancellationToken) is { } pong;
                }
                catch { }

                if (existedContextPingable)
                {
                    logger.LogWarning("New connection for {App} worker {Id} {Name} connected. But the old instance is still alive. Aborting.", appName, id, context.Name);
                    context.Context.Abort("Connection conflict");
                    return;
                }

                WorkerContexts.TryRemove(id, out _);
            }

            if (!WorkerContexts.TryAdd(id, context))
            {
                context.Context.Abort("Connection conflict");
                return;
            }

            logger.LogInformation("Worker {Id} {Name} for {App} has connected.", id, context.Name, appName);

            context.CancellationToken.Register(() =>
            {
                Task.Run(async () =>
                {
                    await WorkerDisconnectedAsync(context);
                });
            });
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// 工作者断开连接
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task WorkerDisconnectedAsync(WorkerContext context)
    {
        var cancellationToken = _runningCTS.Token;
        var id = context.Id;

        //直接锁，方便快捷
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            context.IsActive = false;

            logger.LogWarning("Worker {Id} {Name} in {App} disconnected.", id, context.Name, appName);
            try
            {
                context.Context.Abort("Connection disconnected");
            }
            catch { }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    #endregion Public 方法
}
