using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Util;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

internal sealed class RabbitMQConnectionProvider : IRabbitMQConnectionProvider, IDisposable
{
    #region Private 字段

    private readonly IAsyncConnectionFactory _connectionFactory;

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

    public RabbitMQConnectionProvider(IOptions<RabbitMQOptions> optionsAccessor)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);

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
            };
        }

        PreferSingleConnection = options.PreferSingleConnection;
        if (PreferSingleConnection)
        {
            _connectionGetSemaphore = new(1, 1);
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
                            || existedConnection is IAutorecoveringConnection)
                        {
                            return existedConnection;
                        }
                        _existedConnection = null;
                    }
                    catch (ObjectDisposedException)
                    {
                        _existedConnection = null;
                    }
                }
                var connection = _connectionFactory.CreateConnection();
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
            return _connectionFactory.CreateConnection();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(RabbitMQConnectionProvider)}-{ObjectTag}";
    }

    #endregion Public 方法

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
