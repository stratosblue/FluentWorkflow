using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

internal sealed class RabbitMQConnectionProvider : IRabbitMQConnectionProvider, IDisposable
{
    #region Private 字段

    private readonly IAsyncConnectionFactory _connectionFactory;

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
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                Uri = options.Uri,
                ClientProvidedName = $"fwf:{FluentWorkflowEnvironment.Description}-{ObjectTag}",
                //ConsumerDispatchConcurrency = ushort.MaxValue,  //6.*客户端需要控制并发访问，暂时不处理。。。 https://github.com/rabbitmq/rabbitmq-dotnet-client/discussions/1721#discussioncomment-11215696
            };
        }

        PreferSingleConnection = options.PreferSingleConnection;
        if (PreferSingleConnection)
        {
            _connectionGetSemaphore = new(1, 1);
        }

        _logger = logger;
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
                            || existedConnection is IAutorecoveringConnection)
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
                var connection = CreateNewConnection();
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
            return CreateNewConnection();
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(RabbitMQConnectionProvider)}-{ObjectTag}";

    private IConnection CreateNewConnection()
    {
        var connection = _connectionFactory.CreateConnection();
        _logger.LogInformation("Created new rabbitmq connection {Connection}.", connection);
        if (connection is IAutorecoveringConnection autorecoveringConnection)
        {
            autorecoveringConnection.ConnectionShutdown += OnConnectionShutdown;
            autorecoveringConnection.RecoverySucceeded += OnRecoverySucceeded;
        }

        return connection;
    }

    #endregion Public 方法

    #region connection events

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs eventArgs)
    {
        if (_isDisposed)
        {
            _logger.LogInformation("Workflow RabbitMQ connection shutdown after dispatcher disposed. {EventArgs}", eventArgs);
        }
        else
        {
            _logger.LogCritical("Workflow RabbitMQ connection shutdown. {EventArgs}", eventArgs);
        }
    }

    private void OnRecoverySucceeded(object? sender, EventArgs eventArgs)
    {
        _logger.LogWarning("Workflow RabbitMQ connection recovery succeeded. {EventArgs}", eventArgs);
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
