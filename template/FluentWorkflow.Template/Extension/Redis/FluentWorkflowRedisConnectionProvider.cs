using FluentWorkflow.Handler;
using StackExchange.Redis;

namespace FluentWorkflow.GenericExtension.TemplateNamespace;

/// <summary>
/// 基于 Redis 的 <see cref="IWorkflowAwaitProcessor"/> 连接提供器
/// </summary>
public interface IFluentWorkflowRedisConnectionProvider
{
    #region Public 方法

    /// <summary>
    /// 获取 <see cref="IConnectionMultiplexer"/>
    /// </summary>
    /// <returns></returns>
    IConnectionMultiplexer Get();

    #endregion Public 方法
}

/// <summary>
/// 默认的 <inheritdoc cref="IFluentWorkflowRedisConnectionProvider"/>
/// </summary>
internal sealed class FluentWorkflowRedisConnectionProvider : IFluentWorkflowRedisConnectionProvider, IDisposable
{
    #region Private 字段

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="FluentWorkflowRedisConnectionProvider"/>
    public FluentWorkflowRedisConnectionProvider(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
        _connectionMultiplexer.Dispose();
    }

    /// <inheritdoc/>
    public IConnectionMultiplexer Get()
    {
        return _connectionMultiplexer;
    }

    #endregion Public 方法
}
