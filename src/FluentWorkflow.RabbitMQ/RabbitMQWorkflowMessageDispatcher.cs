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

    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    private readonly IObjectSerializer _objectSerializer;

    private readonly IDisposable? _optionsMonitorDisposer;

    private readonly IRabbitMQChannelPool _rabbitMQChannelPool;

    private bool _disposed;

    private RabbitMQOptions _rabbitMQOptions;

    #endregion Private 字段

    #region Public 构造函数

    public RabbitMQWorkflowMessageDispatcher(IRabbitMQChannelPool rabbitMQChannelPool,
                                             IWorkflowDiagnosticSource diagnosticSource,
                                             IObjectSerializer objectSerializer,
                                             IOptionsMonitor<RabbitMQOptions> rabbitMQOptionsMonitor,
                                             ILogger<RabbitMQWorkflowMessageDispatcher> logger)
        : base(diagnosticSource, logger)
    {
        _rabbitMQChannelPool = rabbitMQChannelPool;
        _objectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));

        _optionsMonitorDisposer = rabbitMQOptionsMonitor.OnChange(options => _rabbitMQOptions = options);
        _rabbitMQOptions = rabbitMQOptionsMonitor.CurrentValue;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _initSemaphore.Dispose();
            _optionsMonitorDisposer?.Dispose();
            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public override async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {
        await base.PublishAsync(message, cancellationToken);
        await SendMessageAsync(message, cancellationToken);
    }

    #endregion Public 方法

    #region Private 方法

    private void SendMessage<TMessage>(IModel channel, TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        var basicProperties = channel.CreateBasicProperties();
        basicProperties.DeliveryMode = 2;
        basicProperties.Headers = new Dictionary<string, object>(2)
        {
            { RabbitMQOptions.EventNameHeaderKey, TMessage.EventName },
            { RabbitMQOptions.WorkflowIdHeaderKey, message.Id }
        };

        var data = _objectSerializer.SerializeToBytes(message);

        cancellationToken.ThrowIfCancellationRequested();

        channel.BasicPublish(exchange: _rabbitMQOptions.ExchangeName ?? RabbitMQOptions.DefaultExchangeName, routingKey: TMessage.EventName, basicProperties, body: data);

        if (channel.NextPublishSeqNo > 0)
        {
            while (!channel.WaitForConfirms(_rabbitMQOptions.PublisherConfirmsCheckTimeout))
            {
                cancellationToken.ThrowIfCancellationRequested();
                //TODO 取消正在确认的消息？
            }
        }
    }

    private async Task SendMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        using var activity = PublisherActivitySource.StartActivity("PublishEventMessage", ActivityKind.Producer);
        if (activity is not null)
        {
            message.Context.SetValue(FluentWorkflowConstants.ContextKeys.ParentTraceContext, TracingContext.Create(activity).Serialize());
        }

        IModel? channel = null;
        try
        {
            channel = await _rabbitMQChannelPool.RentAsync(cancellationToken);
            SendMessage(channel, message, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }
        finally
        {
            if (channel is not null)
            {
                _rabbitMQChannelPool.Return(channel);
            }
        }
    }

    #endregion Private 方法
}
