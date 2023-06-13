using System.Diagnostics;
using System.Text.Json;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Interface;
using FluentWorkflow.RabbitMQ;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FluentWorkflow;

internal sealed class RabbitMQWorkflowMessageDispatcher
    : WorkflowMessageDispatcher, IWorkflowMessageDispatcher, IDisposable
{
    #region Private 字段

    private readonly IRabbitMQConnectionProvider _connectionProvider;

    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    private readonly object _syncRoot = new();

    private IModel? _channel;

    private IConnection? _connection;

    private bool _disposed;

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

    public RabbitMQWorkflowMessageDispatcher(IRabbitMQConnectionProvider connectionProvider, IWorkflowDiagnosticSource diagnosticSource, ILogger<RabbitMQWorkflowMessageDispatcher> logger) : base(diagnosticSource, logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
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
        basicProperties.Headers = new Dictionary<string, object>(1)
        {
            { RabbitMQOptions.EventNameHeaderKey, TMessage.EventName }
        };

        var data = JsonSerializer.SerializeToUtf8Bytes(message, SystemTextJsonObjectSerializer.JsonSerializerOptions);

        Channel.BasicPublish(exchange: RabbitMQOptions.DefaultExchangeName, routingKey: TMessage.EventName, basicProperties, body: data);
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
                Logger.LogInformation("Creating channel.");

                var connection = await _connectionProvider.GetAsync(cancellationToken);

                if (connection is IAutorecoveringConnection autorecoveringConnection)
                {
                    autorecoveringConnection.RecoverySucceeded += (sender, e) =>
                    {
                        Logger.LogWarning("Connection recovery succeeded.");
                    };
                }

                connection.ConnectionShutdown += (sender, e) =>
                {
                    Logger.LogWarning("Connection shutdown.");
                };

                _connection = connection;
            }
        }
        catch (Exception ex)
        {
            _initSemaphore.Release();
            Logger.LogError(ex, "Init channel failed.");
            throw;
        }

        await SendMessageAsync(message, cancellationToken);
    }

    #endregion Private 方法
}
