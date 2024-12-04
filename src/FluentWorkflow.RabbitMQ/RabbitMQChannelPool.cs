using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <inheritdoc cref="IRabbitMQChannelPool"/>
internal sealed class RabbitMQChannelPool : IRabbitMQChannelPool, IDisposable
{
    #region Private 字段

    private readonly IRabbitMQConnectionProvider _connectionProvider;

    private readonly ILogger _logger;

    private readonly IDisposable? _optionsMonitorDisposer;

    private readonly ConcurrentQueue<IChannel> _pooledChannels = new();

    private uint _channelPoolMaxSize;

    private IConnection? _connection;

    private int _currentPoolSize = 0;

    private bool _disposed;

    private RabbitMQOptions _options;

    #endregion Private 字段

    #region Public 构造函数

    public RabbitMQChannelPool(IRabbitMQConnectionProvider connectionProvider,
                               IOptionsMonitor<RabbitMQOptions> rabbitMQOptionsMonitor,
                               ILogger<RabbitMQChannelPool> logger)
    {
        ArgumentNullException.ThrowIfNull(connectionProvider);
        ArgumentNullException.ThrowIfNull(rabbitMQOptionsMonitor);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionProvider = connectionProvider;
        _logger = logger;

        _optionsMonitorDisposer = rabbitMQOptionsMonitor.OnChange(options =>
        {
            _options = options;
            _channelPoolMaxSize = options.ChannelPoolMaxSize;
        });

        _options = rabbitMQOptionsMonitor.CurrentValue;
        _channelPoolMaxSize = rabbitMQOptionsMonitor.CurrentValue.ChannelPoolMaxSize;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public ValueTask<IChannel> RentAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        while (_pooledChannels.TryDequeue(out var channel))
        {
            if (channel.IsOpen)
            {
                _logger.LogDebug("Provide existed channel {Channel}.", channel);

                Interlocked.Decrement(ref _currentPoolSize);

                return ValueTask.FromResult(channel);
            }

            _logger.LogDebug("Pooled channel {Channel} is invalid. Drop it.", channel);

            channel.Dispose();
            Interlocked.Decrement(ref _currentPoolSize);

            cancellationToken.ThrowIfCancellationRequested();
        }

        return InnerRentAsync(cancellationToken);

        async ValueTask<IChannel> InnerRentAsync(CancellationToken cancellationToken = default)
        {
            var connection = await EnsureConnectionAsync(cancellationToken);

            var createChannelOptions = new CreateChannelOptions(publisherConfirmationsEnabled: _options.PublisherConfirms,
                                                                publisherConfirmationTrackingEnabled: _options.PublisherConfirms,
                                                                outstandingPublisherConfirmationsRateLimiter: null,
                                                                consumerDispatchConcurrency: null);

            var channel = await connection.CreateChannelAsync(createChannelOptions, cancellationToken: cancellationToken);

            _logger.LogInformation("Created new channel {Channel}.", channel);

            return channel;
        }
    }

    /// <inheritdoc/>
    public void Return(IChannel channel)
    {
        if (Interlocked.Increment(ref _currentPoolSize) is { } currentPoolSize
            && currentPoolSize <= _channelPoolMaxSize
            && channel.IsOpen)
        {
            _logger.LogDebug("Return channel {Channel} into pool success. CurrentPoolSize: {PoolSize}.", channel, currentPoolSize);
            _pooledChannels.Enqueue(channel);
            return;
        }

        currentPoolSize = Interlocked.Decrement(ref _currentPoolSize);

        _logger.LogDebug("Return channel {Channel}[{IsOpen}] into pool fail. Drop it. CurrentPoolSize: {PoolSize}.", channel, channel.IsOpen, currentPoolSize);

        channel.Dispose();

        Debug.Assert(_currentPoolSize >= 0);
    }

    #endregion Public 方法

    #region Private 方法

    private ValueTask<IConnection> EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (_connection is { } connection
            && connection.IsOpen)
        {
            return ValueTask.FromResult(connection);
        }

        return InnerEnsureConnectionAsync(cancellationToken);

        async ValueTask<IConnection> InnerEnsureConnectionAsync(CancellationToken cancellationToken)
        {
            DisposeConnection();
            return await _connectionProvider.GetAsync(cancellationToken);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    #endregion Private 方法

    #region Dispose

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        _optionsMonitorDisposer?.Dispose();

        _channelPoolMaxSize = 0;

        while (_pooledChannels.TryDequeue(out var channel))
        {
            channel.Dispose();
        }

        DisposeConnection();
    }

    private void DisposeConnection()
    {
        if (Interlocked.Exchange(ref _connection, null) is { } connection)
        {
            _logger.LogDebug("Dispose connection {Connection}.", connection);
            connection.Dispose();
        }
    }

    #endregion Dispose
}
