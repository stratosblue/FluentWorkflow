using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FluentWorkflow.RabbitMQ;

internal sealed class RabbitMQConnectionProvider : IRabbitMQConnectionProvider, IDisposable
{
    #region Private 字段

    private readonly IConnectionFactory _connectionFactory;

    private readonly ILogger _logger;

    private SemaphoreSlim? _connectionGetSemaphore;

    private IConnection? _existedConnection;

    private bool _isDisposed;

    #endregion Private 字段

    #region Public 属性

    public string ObjectTag { get; }

    [MemberNotNull(nameof(_connectionGetSemaphore))]
    public bool PreferSingleConnection { get; }

    #endregion Public 属性

    #region Public 构造函数

    public RabbitMQConnectionProvider(IOptions<RabbitMQOptions> optionsAccessor,
                                      ILogger<RabbitMQConnectionProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);
        ArgumentNullException.ThrowIfNull(logger);

        ObjectTag = ObjectTagUtil.GetHashCodeTag(this);

        var options = optionsAccessor.Value;
        if (options.ConnectionFactory is not null)
        {
            _connectionFactory = options.ConnectionFactory;
        }
        else
        {
            if (options.Uri is null)
            {
                throw new ArgumentException($"{nameof(RabbitMQOptions)}.{nameof(RabbitMQOptions.Uri)} and {nameof(RabbitMQOptions)}.{nameof(RabbitMQOptions.ConnectionFactory)} can not be null both.");
            }
            _connectionFactory = new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = true,
                Uri = options.Uri,
                ClientProvidedName = $"fwf:{FluentWorkflowEnvironment.Description}-{ObjectTag}",
                TopologyRecoveryEnabled = true,
                ConsumerDispatchConcurrency = GetConsumerDispatchConcurrency(),
            };
        }

        PreferSingleConnection = options.PreferSingleConnection;
        if (PreferSingleConnection)
        {
            _connectionGetSemaphore = new(1, 1);
        }

        _logger = logger;

        static ushort GetConsumerDispatchConcurrency()
        {
            //避免阻塞执行，7.*客户端是确认线程安全的 https://github.com/rabbitmq/rabbitmq-dotnet-client/discussions/1721#discussioncomment-11215696
            //7.0客户端内部由Task列表来控制并行数量，过大的值会导致Task过多，尝试在此控制输出一个较大的较为合理的值
            //由于异步执行并不一定占用线程，所以考虑给出一个大于核心数的较大值
            var concurrency = Environment.ProcessorCount * 3;
            if (concurrency < 32)
            {
                return 32;
            }
            return (ushort)concurrency;
        }
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public async Task<IConnection> GetAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (PreferSingleConnection)
        {
            await _connectionGetSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (_existedConnection is { } existedConnection)
                {
                    try
                    {
                        var shutdownEventArgs = existedConnection.CloseReason;
                        if (shutdownEventArgs is null
                            || existedConnection is IRecoverable)
                        {
                            _logger.LogDebug("Provide existed rabbitmq connection {Connection}.", existedConnection);
                            return existedConnection;
                        }
                        _existedConnection = null;
                    }
                    catch (ObjectDisposedException)
                    {
                        _existedConnection = null;
                    }
                }
                var connection = await CreateNewConnectionAsync(cancellationToken);
                _existedConnection = connection;
                return connection;
            }
            finally
            {
                _connectionGetSemaphore.Release();
            }
        }
        else
        {
            return await CreateNewConnectionAsync(cancellationToken); ;
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(RabbitMQConnectionProvider)}-{ObjectTag}";

    private async Task<IConnection> CreateNewConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        _logger.LogInformation("Created new rabbitmq connection {Connection}.", connection);
        if (connection is IRecoverable recoverable)
        {
            connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
            recoverable.RecoveryAsync += OnRecoverySucceededAsync;
        }

        return connection;
    }

    #endregion Public 方法

    #region connection events

    private Task OnConnectionShutdownAsync(object? sender, ShutdownEventArgs eventArgs)
    {
        if (_isDisposed)
        {
            _logger.LogInformation("Workflow RabbitMQ connection shutdown after dispatcher disposed. {EventArgs}", eventArgs);
        }
        else
        {
            _logger.LogCritical("Workflow RabbitMQ connection shutdown. {EventArgs}", eventArgs);
        }

        return Task.CompletedTask;
    }

    private Task OnRecoverySucceededAsync(object? sender, AsyncEventArgs eventArgs)
    {
        _logger.LogWarning("Workflow RabbitMQ connection recovery succeeded. {EventArgs}", eventArgs);
        return Task.CompletedTask;
    }

    #endregion connection events

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~RabbitMQConnectionProvider()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _connectionGetSemaphore?.Dispose();
            _connectionGetSemaphore = null;
            _isDisposed = true;
        }
    }

    #endregion IDisposable
}
