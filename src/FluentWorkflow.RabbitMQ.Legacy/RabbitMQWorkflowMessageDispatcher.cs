using System.ComponentModel;
using System.Diagnostics;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Interface;
using FluentWorkflow.RabbitMQ;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FluentWorkflow;

/// <summary>
/// 基于RabbitMQ的 <inheritdoc cref="WorkflowMessageDispatcher"/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public class RabbitMQWorkflowMessageDispatcher
    : WorkflowMessageDispatcher, IWorkflowMessageDispatcher, IDisposable
{
    #region Private 字段

    private readonly IDisposable? _optionsMonitorDisposer;

    private bool _disposed;

    #endregion Private 字段

    #region Protected 属性

    /// <inheritdoc cref="IRabbitMQExchangeSelector"/>
    protected IRabbitMQExchangeSelector ExchangeSelector { get; }

    /// <inheritdoc cref="IObjectSerializer"/>
    protected IObjectSerializer ObjectSerializer { get; }

    /// <inheritdoc cref="RabbitMQOptions"/>
    protected RabbitMQOptions Options { get; private set; }

    /// <inheritdoc cref="IRabbitMQChannelPool"/>
    protected IRabbitMQChannelPool RabbitMQChannelPool { get; }

    #endregion Protected 属性

    #region Public 构造函数

    /// <inheritdoc cref="RabbitMQWorkflowMessageDispatcher"/>
    public RabbitMQWorkflowMessageDispatcher(IRabbitMQChannelPool rabbitMQChannelPool,
                                             IWorkflowDiagnosticSource diagnosticSource,
                                             IObjectSerializer objectSerializer,
                                             IRabbitMQExchangeSelector exchangeSelector,
                                             IOptionsMonitor<RabbitMQOptions> rabbitMQOptionsMonitor,
                                             ILogger<RabbitMQWorkflowMessageDispatcher> logger)
        : base(diagnosticSource, logger)
    {
        RabbitMQChannelPool = rabbitMQChannelPool ?? throw new ArgumentNullException(nameof(rabbitMQChannelPool));
        ObjectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        ExchangeSelector = exchangeSelector ?? throw new ArgumentNullException(nameof(exchangeSelector));

        _optionsMonitorDisposer = rabbitMQOptionsMonitor.OnChange(options => Options = options);
        Options = rabbitMQOptionsMonitor.CurrentValue;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
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

        var exchange = ExchangeSelector.GetExchange(message, cancellationToken);

        var data = ObjectSerializer.SerializeToBytes(message);

        cancellationToken.ThrowIfCancellationRequested();

        channel.BasicPublish(exchange: exchange,
                             routingKey: TMessage.EventName,
                             basicProperties,
                             body: data);

        if (channel.NextPublishSeqNo > 0)
        {
            while (!channel.WaitForConfirms(Options.PublisherConfirmsCheckTimeout))
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
            channel = await RabbitMQChannelPool.RentAsync(cancellationToken);
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
                RabbitMQChannelPool.Return(channel);
            }
        }
    }

    #endregion Private 方法
}
