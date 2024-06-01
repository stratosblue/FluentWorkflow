﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentWorkflow.Build;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Interface;
using FluentWorkflow.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

internal class RabbitMQBootstrapper : IFluentWorkflowBootstrapper
{
    #region Private 字段

    private readonly IRabbitMQConnectionProvider _connectionProvider;

    private readonly ILogger _consumeLogger;

    private readonly IWorkflowDiagnosticSource _diagnosticSource;

    private readonly ImmutableDictionary<string, WorkflowEventInvokerDescriptor[]> _eventSubscribeDescriptors;

    private readonly ILogger _logger;

    private readonly IObjectSerializer _objectSerializer;

    private readonly RabbitMQOptions _options;

    private readonly CancellationToken _runningCancellationToken;

    private readonly CancellationTokenSource _runningCancellationTokenSource;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private bool _disposed;

    #endregion Private 字段

    #region Public 构造函数

    public RabbitMQBootstrapper(IRabbitMQConnectionProvider connectionProvider,
                                WorkflowBuildStateCollection workflowBuildStates,
                                IServiceScopeFactory serviceScopeFactory,
                                IObjectSerializer objectSerializer,
                                IWorkflowDiagnosticSource diagnosticSource,
                                ILoggerFactory loggerFactory,
                                IOptions<RabbitMQOptions> optionsAccessor)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _objectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        _diagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
        _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _eventSubscribeDescriptors = workflowBuildStates.SelectMany(m => m)
                                                        .ToImmutableDictionary(m => m.EventName, m => m.ToArray());
        _runningCancellationTokenSource = new();
        _runningCancellationToken = _runningCancellationTokenSource.Token;
        _logger = loggerFactory.CreateLogger<RabbitMQBootstrapper>();
        _consumeLogger = loggerFactory.CreateLogger(FluentWorkflowConstants.DefaultConsumerLoggerName);
    }

    #endregion Public 构造函数

    #region Public 方法

    public ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return ValueTask.CompletedTask;
        }
        _logger.LogInformation("Disposing workflow RabbitMQ message dispatcher.");
        try
        {
            _runningCancellationTokenSource.Cancel();
        }
        catch { }
        try
        {
            _runningCancellationTokenSource.Dispose();
        }
        catch { }
        _disposed = true;
        return ValueTask.CompletedTask;
    }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start initializing workflow RabbitMQ message dispatcher.");

        var connection = await _connectionProvider.GetAsync(cancellationToken);

        var exchangeName = _options.ExchangeName ?? RabbitMQOptions.DefaultExchangeName;

        //global channel
        var channel = connection.CreateModel();
        //声明交换机
        channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, true, false, null);

        var defaultConsumeQueueName = NormalizConsumeQueueName(_options.ConsumeQueueName ?? Assembly.GetEntryAssembly()?.GetName().Name?.ToLowerInvariant());

        var queueArguments = GetQueueArguments(defaultConsumeQueueName);

        //声明默认队列
        channel.QueueDeclare(defaultConsumeQueueName, true, false, false, queueArguments);

        if (_options.GlobalQos > 0)
        {
            channel.BasicQos(0, _options.GlobalQos, false);
        }

        var defaultConsumeDescriptors = new Dictionary<string, ConsumeDescriptor>();
        var standaloneEventNames = new HashSet<string>();
        foreach (var (eventName, invokerDescriptors) in _eventSubscribeDescriptors)
        {
            if (_options.MessageHandleOptions.TryGetValue(eventName, out var messageHandleOptions)
                && messageHandleOptions.Qos > 0)
            {
                var standaloneQueueName = $"{defaultConsumeQueueName}:{eventName}";

                _logger.LogInformation("Use standalone channel consume workflow messages. EventName: {EventName}. QueueName: {QueueName}.", eventName, standaloneQueueName);

                var standaloneChannel = connection.CreateModel();
                var consumeDescriptor = new ConsumeDescriptor(EventName: eventName,
                                                              InvokerDescriptors: invokerDescriptors,
                                                              RequeuePolicy: messageHandleOptions.RequeuePolicy,
                                                              RequeueDelay: messageHandleOptions.RequeueDelay);
                SetupStandAloneConsumer(standaloneChannel, standaloneQueueName, consumeDescriptor, messageHandleOptions);
                //从默认队列解绑
                channel.QueueUnbind(defaultConsumeQueueName, exchangeName, eventName, null);
                //绑定到独立队列
                channel.QueueBind(standaloneQueueName, exchangeName, eventName, null);
                standaloneEventNames.Add(eventName);
            }
            else
            {
                var consumeDescriptor = new ConsumeDescriptor(EventName: eventName,
                                                              InvokerDescriptors: invokerDescriptors,
                                                              RequeuePolicy: messageHandleOptions?.RequeuePolicy ?? MessageRequeuePolicy.Unlimited,
                                                              RequeueDelay: messageHandleOptions?.RequeueDelay ?? RabbitMQOptions.MessageRequeueDelay);
                defaultConsumeDescriptors.Add(eventName, consumeDescriptor);
                //绑定到默认队列
                channel.QueueBind(defaultConsumeQueueName, exchangeName, eventName, null);
            }
        }

        if (defaultConsumeDescriptors.Count > 0)
        {
            _logger.LogInformation("Use default channel consume workflow messages. Consumer count: {ConsumerCount}.", defaultConsumeDescriptors.Count);

            var consumeDescriptors = defaultConsumeDescriptors.ToImmutableDictionary();

            var consumer = new MultipleEventMessageConsumer(consumeDescriptors,
                                                            _options,
                                                            standaloneEventNames,
                                                            channel,
                                                            _serviceScopeFactory,
                                                            _objectSerializer,
                                                            _diagnosticSource,
                                                            _consumeLogger,
                                                            _runningCancellationToken);

            channel.BasicConsume(queue: defaultConsumeQueueName,
                                 autoAck: false,
                                 consumerTag: $"fwf:{FluentWorkflowEnvironment.Description}-{ObjectTagUtil.GetHashCodeTag(this)}",
                                 consumer: consumer);
        }
    }

    #endregion Public 方法

    #region Private 方法

    private Dictionary<string, object> GetQueueArguments(string standaloneQueueName)
    {
        var arguments = new Dictionary<string, object>()
        {
            //see https://www.rabbitmq.com/docs/consumers#acknowledgement-timeout
            { "x-consumer-timeout", (uint)TimeSpan.FromHours(1).TotalMilliseconds },
        };

        _options.QueueArgumentsSetup?.Invoke(standaloneQueueName, arguments);

        return arguments;
    }

    private string NormalizConsumeQueueName(string? queueName)
    {
        ThrowIfQueueNameInvalid(queueName);

        var finalQueueName = _options.ConsumeQueueNameNormalizer?.Invoke(queueName) ?? queueName;

        ThrowIfQueueNameInvalid(finalQueueName);

        return finalQueueName;

        static void ThrowIfQueueNameInvalid([NotNull] string? finalQueueName)
        {
            if (string.IsNullOrWhiteSpace(finalQueueName))
            {
                throw new InvalidOperationException($"Invalid workflow consume queue name \"{finalQueueName}\"");
            }
        }
    }

    private void SetupStandAloneConsumer(IModel channel, string standaloneQueueName, ConsumeDescriptor consumeDescriptor, MessageHandleOptions messageHandleOptions)
    {
        var queueArguments = GetQueueArguments(standaloneQueueName);

        channel.QueueDeclare(standaloneQueueName, true, false, false, queueArguments);
        channel.BasicQos(0, messageHandleOptions.Qos, false);

        var consumer = new StandAloneEventMessageConsumer(consumeDescriptor,
                                                          channel,
                                                          _serviceScopeFactory,
                                                          _objectSerializer,
                                                          _diagnosticSource,
                                                          _consumeLogger,
                                                          _runningCancellationToken);

        channel.BasicConsume(queue: standaloneQueueName,
                             autoAck: false,
                             consumerTag: $"fwf:{FluentWorkflowEnvironment.Description}-{ObjectTagUtil.GetHashCodeTag(this)}",
                             consumer: consumer);
    }

    #endregion Private 方法
}
