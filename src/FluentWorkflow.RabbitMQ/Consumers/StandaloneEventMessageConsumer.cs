﻿using FluentWorkflow.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 独立事件消息消费者
/// </summary>
internal sealed class StandaloneEventMessageConsumer : EventMessageBasicConsumer
{
    #region Public 属性

    public ConsumeDescriptor ConsumeDescriptor { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="StandaloneEventMessageConsumer"/>
    public StandaloneEventMessageConsumer(ConsumeDescriptor consumeDescriptor,
                                          IChannel channel,
                                          IServiceScopeFactory serviceScopeFactory,
                                          IObjectSerializer objectSerializer,
                                          IWorkflowDiagnosticSource diagnosticSource,
                                          ILogger logger,
                                          CancellationToken runningToken)
        : base(channel, serviceScopeFactory, objectSerializer, diagnosticSource, logger, runningToken)
    {
        ConsumeDescriptor = consumeDescriptor ?? throw new ArgumentNullException(nameof(consumeDescriptor));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task InternalHandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        var eventName = "UnknownEventName";
        if (properties?.Headers is { } headers
            && TryGetEventName(headers, ref eventName)
            && string.Equals(ConsumeDescriptor.EventName, eventName))
        {
            return ConsumeMessageAsync(ConsumeDescriptor, consumerTag, deliveryTag, redelivered, exchange, routingKey, body);
        }

        return HandleErrorMessageAsync(eventName: eventName,
                                       consumerTag: consumerTag,
                                       deliveryTag: deliveryTag,
                                       redelivered: redelivered,
                                       exchange: exchange,
                                       routingKey: routingKey,
                                       policy: ConsumeDescriptor.RequeuePolicy,
                                       delay: ConsumeDescriptor.RequeueDelay,
                                       forceNotRequeue: false);
    }

    #endregion Protected 方法
}
