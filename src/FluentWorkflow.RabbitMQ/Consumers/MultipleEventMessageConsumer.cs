﻿using System.Collections.Immutable;
using FluentWorkflow.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 复合事件消息消费者
/// </summary>
internal sealed class MultipleEventMessageConsumer : EventMessageBasicConsumer
{
    #region Private 字段

    private readonly TimeSpan _errorMessageRequeueDelay;

    private readonly MessageRequeuePolicy _errorMessageRequeuePolicy;

    #endregion Private 字段

    #region Public 属性

    public ImmutableDictionary<string, ConsumeDescriptor> ConsumeDescriptors { get; }

    public ImmutableHashSet<string> StandaloneEventNames { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="MultipleEventMessageConsumer"/>
    public MultipleEventMessageConsumer(ImmutableDictionary<string, ConsumeDescriptor> consumeDescriptors,
                                        RabbitMQOptions options,
                                        HashSet<string> standaloneEventNames,
                                        IChannel channel,
                                        IServiceScopeFactory serviceScopeFactory,
                                        IObjectSerializer objectSerializer,
                                        IWorkflowDiagnosticSource diagnosticSource,
                                        ILogger logger,
                                        CancellationToken runningToken)
        : base(channel, serviceScopeFactory, objectSerializer, diagnosticSource, logger, runningToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        ConsumeDescriptors = consumeDescriptors ?? throw new ArgumentNullException(nameof(consumeDescriptors));
        StandaloneEventNames = standaloneEventNames?.ToImmutableHashSet() ?? throw new ArgumentNullException(nameof(standaloneEventNames));
        _errorMessageRequeuePolicy = options.ErrorMessageRequeuePolicy;
        _errorMessageRequeueDelay = options.ErrorMessageRequeueDelay;
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task InternalHandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        var eventName = "UnknownEventName";
        if (properties?.Headers is { } headers
            && TryGetEventName(headers, ref eventName)
            && ConsumeDescriptors.TryGetValue(eventName, out var consumeDescriptor))
        {
            return ConsumeMessageAsync(consumeDescriptor, consumerTag, deliveryTag, redelivered, exchange, routingKey, body);
        }

        var forceNotRequeue = StandaloneEventNames.Contains(eventName);

        if (forceNotRequeue)
        {
            Logger.LogWarning("Consume message error on received {EventName}. Not found consumer for it but find this message bind in Qos queue. Not requeue it. If changed the qos for the message, you may need to manually unbind the routing key of the queue. [{ConsumerTag}] Routing: {Exchange} -> {RoutingKey}.", eventName, consumerTag, exchange, routingKey);
        }

        return HandleErrorMessageAsync(eventName: eventName,
                                       consumerTag: consumerTag,
                                       deliveryTag: deliveryTag,
                                       redelivered: redelivered,
                                       exchange: exchange,
                                       routingKey: routingKey,
                                       policy: _errorMessageRequeuePolicy,
                                       delay: _errorMessageRequeueDelay,
                                       forceNotRequeue: forceNotRequeue);
    }

    #endregion Protected 方法
}
