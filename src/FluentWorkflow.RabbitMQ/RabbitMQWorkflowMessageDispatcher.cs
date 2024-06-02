using System.Diagnostics;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Interface;
using FluentWorkflow.RabbitMQ;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FluentWorkflow;

internal sealed class RabbitMQWorkflowMessageDispatcher
    : WorkflowMessageDispatcher, IWorkflowMessageDispatcher, IDisposable
{
    #region Private 字段

    private readonly IRabbitMQConnectionProvider _connectionProvider;

    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    private readonly IObjectSerializer _objectSerializer;

    private readonly IDisposable? _optionsMonitorDisposer;

    private readonly object _syncRoot = new();

    private IModel? _channel;

    private IConnection? _connection;

    private bool _disposed;

    private RabbitMQOptions _rabbitMQOptions;

    #endregion Private 字段

    #region Private 属性

    private IModel Channel
    {
        get
        {
            if (_channel is null
                || _channel.IsClosed)
            {
                lock (_syncRoot)
                {
                    if (_channel is null
                        || _channel.IsClosed)
                    {
                        _channel = _connection!.CreateModel();
                    }
                }
            }
            return _channel;
        }
    }

    #endregion Private 属性

    #region Public 构造函数

    public RabbitMQWorkflowMessageDispatcher(IRabbitMQConnectionProvider connectionProvider,
                                             IWorkflowDiagnosticSource diagnosticSource,
                                             IObjectSerializer objectSerializer,
                                             IOptionsMonitor<RabbitMQOptions> rabbitMQOptionsMonitor,
                                             ILogger<RabbitMQWorkflowMessageDispatcher> logger)
        : base(diagnosticSource, logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _objectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));

        _optionsMonitorDisposer = rabbitMQOptionsMonitor.OnChange(options => _rabbitMQOptions = options);
        _rabbitMQOptions = rabbitMQOptionsMonitor.CurrentValue;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Dispose()
    {
        if (!_disposed)
        {
            _initSemaphore.Dispose();
            _channel?.Dispose();
            _connection?.Dispose();
            _optionsMonitorDisposer?.Dispose();
            _disposed = true;
        }
    }

    public override async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {
        await base.PublishAsync(message, cancellationToken);

        if (_connection is not null)
        {
            await SendMessageAsync(message, cancellationToken);
        }
        else
        {
            await SendMessageWithCreateChannelAsync(message, cancellationToken);
        }
    }

    #endregion Public 方法

    #region Private 方法

    private Task SendMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        using var activity = PublisherActivitySource.StartActivity("PublishEventMessage", ActivityKind.Producer);
        if (activity is not null)
        {
            message.Context.SetValue(FluentWorkflowConstants.ContextKeys.ParentTraceContext, TracingContext.Create(activity).Serialize());
        }

        var basicProperties = Channel.CreateBasicProperties();
        basicProperties.DeliveryMode = 2;
        basicProperties.Headers = new Dictionary<string, object>(2)
        {
            { RabbitMQOptions.EventNameHeaderKey, TMessage.EventName },
            { RabbitMQOptions.WorkflowIdHeaderKey, message.Id }
        };

        var data = _objectSerializer.SerializeToBytes(message);

        Channel.BasicPublish(exchange: _rabbitMQOptions.ExchangeName ?? RabbitMQOptions.DefaultExchangeName, routingKey: TMessage.EventName, basicProperties, body: data);
        return Task.CompletedTask;
    }

    private async Task SendMessageWithCreateChannelAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        await _initSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_connection is null)
            {
                Logger.LogInformation("Creating workflow RabbitMQ channel.");

                var connection = await _connectionProvider.GetAsync(cancellationToken);

                if (connection is IAutorecoveringConnection autorecoveringConnection)
                {
                    autorecoveringConnection.RecoverySucceeded += (sender, e) =>
                    {
                        Logger.LogWarning("Workflow RabbitMQ connection recovery succeeded.");
                    };
                }

                connection.ConnectionShutdown += (sender, e) =>
                {
                    Logger.LogWarning("Workflow RabbitMQ connection shutdown.");
                };

                _connection = connection;
            }
        }
        catch (Exception ex)
        {
            _initSemaphore.Release();
            Logger.LogError(ex, "Init workflow RabbitMQ channel failed.");
            throw;
        }

        await SendMessageAsync(message, cancellationToken);
    }

    #endregion Private 方法
}
