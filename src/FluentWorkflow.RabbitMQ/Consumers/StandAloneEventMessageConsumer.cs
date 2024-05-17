﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 独立事件消息消费者
/// </summary>
internal sealed class StandAloneEventMessageConsumer : EventMessageBasicConsumer
{
    #region Public 属性

    public ConsumeDescriptor ConsumeDescriptor { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="StandAloneEventMessageConsumer"/>
    public StandAloneEventMessageConsumer(ConsumeDescriptor consumeDescriptor,
                                          IModel model,
                                          IServiceScopeFactory serviceScopeFactory,
                                          IObjectSerializer objectSerializer,
                                          ILogger logger,
                                          CancellationToken runningToken)
        : base(model, serviceScopeFactory, objectSerializer, logger, runningToken)
    {
        ConsumeDescriptor = consumeDescriptor ?? throw new ArgumentNullException(nameof(consumeDescriptor));
    }

    #endregion Public 构造函数

    #region Public 方法

    public override Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
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

    #endregion Public 方法
}
