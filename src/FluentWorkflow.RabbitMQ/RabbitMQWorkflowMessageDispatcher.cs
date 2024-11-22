using System.ComponentModel;
using System.Diagnostics;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Interface;
using FluentWorkflow.RabbitMQ;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FluentWorkflow;

/// <summary>
/// 基于RabbitMQ的 <inheritdoc cref="WorkflowMessageDispatcher"/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public class RabbitMQWorkflowMessageDispatcher
    : WorkflowMessageDispatcher, IWorkflowMessageDispatcher, IDisposable
{
    #region Protected 属性

    /// <inheritdoc cref="IRabbitMQExchangeSelector"/>
    protected IRabbitMQExchangeSelector ExchangeSelector { get; }

    /// <inheritdoc cref="IObjectSerializer"/>
    protected IObjectSerializer ObjectSerializer { get; }

    /// <inheritdoc cref="IRabbitMQChannelPool"/>
    protected IRabbitMQChannelPool RabbitMQChannelPool { get; }

    #endregion Protected 属性

    #region Public 构造函数

    /// <inheritdoc cref="RabbitMQWorkflowMessageDispatcher"/>
    public RabbitMQWorkflowMessageDispatcher(IRabbitMQChannelPool rabbitMQChannelPool,
                                             IWorkflowDiagnosticSource diagnosticSource,
                                             IRabbitMQExchangeSelector exchangeSelector,
                                             IObjectSerializer objectSerializer,
                                             ILogger<RabbitMQWorkflowMessageDispatcher> logger)
        : base(diagnosticSource, logger)
    {
        RabbitMQChannelPool = rabbitMQChannelPool ?? throw new ArgumentNullException(nameof(rabbitMQChannelPool));
        ExchangeSelector = exchangeSelector ?? throw new ArgumentNullException(nameof(exchangeSelector));
        ObjectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public override async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {
        await base.PublishAsync(message, cancellationToken);
        await SendMessageAsync(message, cancellationToken);
    }

    #endregion Public 方法

    #region Private 方法

    private async Task SendMessageAsync<TMessage>(IChannel channel, TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        var basicProperties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>(2)
            {
                { RabbitMQOptions.EventNameHeaderKey, TMessage.EventName },
                { RabbitMQOptions.WorkflowIdHeaderKey, message.Id }
            }
        };

        var exchange = await ExchangeSelector.GetExchangeAsync(message, cancellationToken);

        var data = ObjectSerializer.SerializeToBytes(message);

        cancellationToken.ThrowIfCancellationRequested();

        await channel.BasicPublishAsync(exchange: exchange,
                                        routingKey: TMessage.EventName,
                                        mandatory: false,
                                        basicProperties: basicProperties,
                                        body: data,
                                        cancellationToken: cancellationToken);
    }

    private async Task SendMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        using var activity = PublisherActivitySource.StartActivity("PublishEventMessage", ActivityKind.Producer);
        if (activity is not null)
        {
            message.Context.SetValue(FluentWorkflowConstants.ContextKeys.ParentTraceContext, TracingContext.Create(activity).Serialize());
        }

        IChannel? channel = null;
        try
        {
            channel = await RabbitMQChannelPool.RentAsync(cancellationToken);
            await SendMessageAsync(channel, message, cancellationToken);
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
