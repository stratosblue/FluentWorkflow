using System.Collections.Immutable;
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

internal sealed class RabbitMQBootstrapper : IFluentWorkflowBootstrapper
{
    #region Private 字段

    private readonly List<ChannelScope> _channelScopes = [];

    private readonly IRabbitMQConnectionProvider _connectionProvider;

    private readonly ILogger _consumeLogger;

    private readonly IWorkflowDiagnosticSource _diagnosticSource;

    private readonly ImmutableDictionary<string, WorkflowEventInvokerDescriptor[]> _eventSubscribeDescriptors;

    private readonly ILogger _logger;

    private readonly IObjectSerializer _objectSerializer;

    private readonly RabbitMQOptions _options;

    private readonly CancellationToken _runningCancellationToken;

    private readonly CancellationTokenSource _runningCancellationTokenSource;

    private readonly IServiceProvider _serviceProvider;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private IConnection? _connection;

    private bool _disposed;

    #endregion Private 字段

    #region Public 属性

    public string ObjectTag { get; }

    #endregion Public 属性

    #region Public 构造函数

    public RabbitMQBootstrapper(IRabbitMQConnectionProvider connectionProvider,
                                WorkflowBuildStateCollection workflowBuildStates,
                                IServiceScopeFactory serviceScopeFactory,
                                IObjectSerializer objectSerializer,
                                IWorkflowDiagnosticSource diagnosticSource,
                                ILoggerFactory loggerFactory,
                                IServiceProvider serviceProvider,
                                IOptions<RabbitMQOptions> optionsAccessor)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _objectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        _diagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _eventSubscribeDescriptors = workflowBuildStates.SelectMany(m => m)
                                                        .ToImmutableDictionary(m => m.EventName, m => m.ToArray());
        _runningCancellationTokenSource = new();
        _runningCancellationToken = _runningCancellationTokenSource.Token;
        _logger = loggerFactory.CreateLogger<RabbitMQBootstrapper>();
        _consumeLogger = loggerFactory.CreateLogger(FluentWorkflowConstants.DefaultConsumerLoggerName);

        ObjectTag = ObjectTagUtil.GetHashCodeTag(this);
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

        _connection?.Dispose();

        foreach (var item in _channelScopes)
        {
            item.Dispose();
        }

        _disposed = true;
        return ValueTask.CompletedTask;
    }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start initializing workflow RabbitMQ message dispatcher.");

        var connection = await _connectionProvider.GetAsync(cancellationToken);

        _connection = connection;

        var exchangeName = _options.ExchangeName ?? RabbitMQOptions.DefaultExchangeName;

        //global channel
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        //声明交换机
        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null, noWait: false, cancellationToken: cancellationToken);

        var defaultConsumeQueueName = NormalizConsumeQueueName(_options.ConsumeQueueName ?? Assembly.GetEntryAssembly()?.GetName().Name?.ToLowerInvariant());

        var queueArguments = GetQueueArguments(defaultConsumeQueueName);

        //声明默认队列
        await channel.QueueDeclareAsync(queue: defaultConsumeQueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments!, noWait: false, cancellationToken: cancellationToken);

        if (_options.GlobalQos > 0)
        {
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _options.GlobalQos, global: false, cancellationToken: cancellationToken);
        }

        var groupedEventNames = new HashSet<string>();

        foreach (var (groupName, messageGroup) in _options.MessageGroups)
        {
            var messageHandleOptions = messageGroup.MessageHandleOptions;

            var groupQueueName = $"{defaultConsumeQueueName}:{groupName}";
            var groupQueueArguments = GetQueueArguments(groupQueueName);
            var groupChannelScope = await messageGroup.ChannelFactory(_serviceProvider, groupQueueName, groupQueueArguments, messageGroup, cancellationToken);

            _channelScopes.Add(groupChannelScope);

            var groupChannel = groupChannelScope.Channel;
            var groupConsumeDescriptors = new Dictionary<string, ConsumeDescriptor>();
            foreach (var message in messageGroup.Messages)
            {
                var eventName = message.Key;
                if (!_eventSubscribeDescriptors.TryGetValue(eventName, out var invokerDescriptors))
                {
                    throw new InvalidOperationException($"The grouped message - \"{eventName}\" has no subscription information.");
                }
                groupedEventNames.Add(eventName);

                //从默认队列解绑
                await channel.QueueUnbindAsync(queue: defaultConsumeQueueName, exchange: exchangeName, routingKey: eventName, arguments: null, cancellationToken: cancellationToken);

                var consumeDescriptor = new ConsumeDescriptor(EventName: eventName,
                                                              InvokerDescriptors: invokerDescriptors,
                                                              RequeuePolicy: messageHandleOptions.RequeuePolicy,
                                                              RequeueDelay: messageHandleOptions.RequeueDelay);
                groupConsumeDescriptors.Add(message.Key, consumeDescriptor);
            }

            var consumer = new MultipleEventMessageConsumer(groupConsumeDescriptors.ToImmutableDictionary(),
                                                            _options,
                                                            [],
                                                            channel,
                                                            _serviceScopeFactory,
                                                            _objectSerializer,
                                                            _diagnosticSource,
                                                            _consumeLogger,
                                                            _runningCancellationToken);

            await groupChannel.BasicConsumeAsync(queue: groupQueueName,
                                                 autoAck: false,
                                                 consumerTag: $"fwf:{FluentWorkflowEnvironment.Description}-{ObjectTag}",
                                                 noLocal: false,
                                                 exclusive: false,
                                                 arguments: null,
                                                 consumer: consumer,
                                                 cancellationToken: cancellationToken);
        }

        var defaultConsumeDescriptors = new Dictionary<string, ConsumeDescriptor>();
        foreach (var (eventName, invokerDescriptors) in _eventSubscribeDescriptors)
        {
            if (_options.MessageGroups.Values.Any(m => m.Messages.ContainsKey(eventName)))
            {
                continue;
            }

            var consumeDescriptor = new ConsumeDescriptor(EventName: eventName,
                                                          InvokerDescriptors: invokerDescriptors,
                                                          RequeuePolicy: MessageRequeuePolicy.Unlimited,
                                                          RequeueDelay: RabbitMQOptions.MessageRequeueDelay);
            defaultConsumeDescriptors.Add(eventName, consumeDescriptor);
            //绑定到默认队列
            await channel.QueueBindAsync(queue: defaultConsumeQueueName, exchange: exchangeName, routingKey: eventName, arguments: null, noWait: false, cancellationToken: cancellationToken);
        }

        if (defaultConsumeDescriptors.Count > 0)
        {
            _logger.LogInformation("Use default channel consume workflow messages. Consumer count: {ConsumerCount}.", defaultConsumeDescriptors.Count);

            var consumeDescriptors = defaultConsumeDescriptors.ToImmutableDictionary();

            var consumer = new MultipleEventMessageConsumer(consumeDescriptors,
                                                            _options,
                                                            groupedEventNames,
                                                            channel,
                                                            _serviceScopeFactory,
                                                            _objectSerializer,
                                                            _diagnosticSource,
                                                            _consumeLogger,
                                                            _runningCancellationToken);

            await channel.BasicConsumeAsync(queue: defaultConsumeQueueName,
                                            autoAck: false,
                                            consumerTag: $"fwf:{FluentWorkflowEnvironment.Description}-{ObjectTag}",
                                            noLocal: false,
                                            exclusive: false,
                                            arguments: null,
                                            consumer: consumer,
                                            cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(RabbitMQBootstrapper)}-{ObjectTag}";
    }

    #endregion Public 方法

    #region Private 方法

    private Dictionary<string, object> GetQueueArguments(string queueName)
    {
        var arguments = new Dictionary<string, object>()
        {
            //see https://www.rabbitmq.com/docs/consumers#acknowledgement-timeout
            { "x-consumer-timeout", (uint)TimeSpan.FromHours(1).TotalMilliseconds },
        };

        _options.QueueArgumentsSetup?.Invoke(queueName, arguments);

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

    #endregion Private 方法
}
