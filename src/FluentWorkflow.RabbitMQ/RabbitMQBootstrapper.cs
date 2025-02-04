using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Build;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.MessageDispatch;
using FluentWorkflow.Util;
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

    private readonly ImmutableDictionary<string, ImmutableArray<WorkflowEventInvokerDescriptor>> _eventSubscribeDescriptors;

    private readonly ILogger _logger;

    private readonly IMessageConsumeDispatcher _messageConsumeDispatcher;

    private readonly IObjectSerializer _objectSerializer;

    private readonly RabbitMQOptions _options;

    private readonly CancellationToken _runningCancellationToken;

    private readonly CancellationTokenSource _runningCancellationTokenSource;

    private readonly IServiceProvider _serviceProvider;

    private IConnection? _connection;

    private bool _disposed;

    #endregion Private 字段

    #region Public 属性

    public string ObjectTag { get; }

    #endregion Public 属性

    #region Public 构造函数

    public RabbitMQBootstrapper(IRabbitMQConnectionProvider connectionProvider,
                                WorkflowBuildStateCollection workflowBuildStates,
                                IMessageConsumeDispatcher messageConsumeDispatcher,
                                IObjectSerializer objectSerializer,
                                IWorkflowDiagnosticSource diagnosticSource,
                                ILoggerFactory loggerFactory,
                                IServiceProvider serviceProvider,
                                IOptions<RabbitMQOptions> optionsAccessor)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _messageConsumeDispatcher = messageConsumeDispatcher ?? throw new ArgumentNullException(nameof(messageConsumeDispatcher));
        _objectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        _diagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _eventSubscribeDescriptors = workflowBuildStates.SelectMany(m => m)
                                                        .ToImmutableDictionary(m => m.EventName, m => m.ToImmutableArray());
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

        GetMessageOptions(out var messageTransmissionTypes, out var messageHandleOptions);

        var connection = await _connectionProvider.GetAsync(cancellationToken);

        _connection = connection;

        var exchangeName = _options.ExchangeName ?? RabbitMQOptions.DefaultExchangeName;

        //global Channel
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

        //处理每个分组
        var allGroupedEventNames = new HashSet<string>();
        foreach (var (groupName, messageGroup) in _options.MessageGroups)
        {
            GetGroupEventNames(messageGroup, allGroupedEventNames, out var groupEventNames);

            var handleOptions = messageGroup.MessageHandleOptions;
            var groupQueueName = $"{defaultConsumeQueueName}:{groupName}";
            var groupQueueArguments = GetQueueArguments(groupQueueName);

            var initializationResult = await messageGroup.GroupInitializationCallback(serviceProvider: _serviceProvider,
                                                                                      queueName: groupQueueName,
                                                                                      queueArguments: groupQueueArguments,
                                                                                      messageConsumeGroup: messageGroup,
                                                                                      eventSubscribeDescriptors: _eventSubscribeDescriptors,
                                                                                      cancellationToken: cancellationToken);

            if (initializationResult.CustomHandled)
            {
                _logger.LogInformation("Message group {GroupName}'s message will handle by user.", groupName);
                continue;
            }

            var groupChannelScope = initializationResult.ChannelScope ?? throw new InvalidOperationException("Non custom message group must has a Channel scope.");
            var groupChannel = groupChannelScope.Channel;

            _channelScopes.Add(groupChannelScope);

            //尝试从本分组的队列解绑非分组的所有消息
            foreach (var eventName in messageTransmissionTypes.Select(static m => m.Key).Where(m => !groupEventNames.Contains(m)))
            {
                await groupChannel.QueueUnbindAsync(queue: groupQueueName, exchange: exchangeName, routingKey: eventName, arguments: null, cancellationToken: cancellationToken);
            }
            //绑定该分组的所有消息
            foreach (var eventName in groupEventNames)
            {
                await channel.QueueBindAsync(queue: groupQueueName, exchange: exchangeName, routingKey: eventName, arguments: null, noWait: false, cancellationToken: cancellationToken);
            }

            await BindChannelConsumerAsync(groupChannel, groupQueueName, groupEventNames.ToImmutableHashSet(), cancellationToken);
        }

        var defaultConsumeEventNames = new HashSet<string>();
        foreach (var eventName in _eventSubscribeDescriptors.Keys)
        {
            if (allGroupedEventNames.Contains(eventName))
            {
                //从默认队列解绑已分组的消息
                await channel.QueueUnbindAsync(queue: defaultConsumeQueueName, exchange: exchangeName, routingKey: eventName, arguments: null, cancellationToken: cancellationToken);
                continue;
            }

            defaultConsumeEventNames.Add(eventName);
            //绑定到默认队列
            await channel.QueueBindAsync(queue: defaultConsumeQueueName, exchange: exchangeName, routingKey: eventName, arguments: null, noWait: false, cancellationToken: cancellationToken);
        }

        if (defaultConsumeEventNames.Count > 0)
        {
            _logger.LogInformation("Use default Channel consume workflow messages. Consumer count: {ConsumerCount}.", defaultConsumeEventNames.Count);
            await BindChannelConsumerAsync(channel, defaultConsumeQueueName, defaultConsumeEventNames.ToImmutableHashSet(), cancellationToken);
        }

        void GetGroupEventNames(MessageConsumeGroup messageGroup, HashSet<string> allGroupedEventNames, out ImmutableHashSet<string> groupEventNames)
        {
            var result = new HashSet<string>();
            foreach (var eventName in messageGroup.Messages.Keys)
            {
                if (!_eventSubscribeDescriptors.ContainsKey(eventName))
                {
                    throw new InvalidOperationException($"The grouped message - \"{eventName}\" has no subscription information.");
                }
                result.Add(eventName);
                allGroupedEventNames.Add(eventName);
            }
            groupEventNames = result.ToImmutableHashSet();
        }

        async Task BindChannelConsumerAsync(IChannel channel, string targetQueue, ImmutableHashSet<string> targetEventNames, CancellationToken cancellationToken)
        {
            var consumer = new EventMessageConsumer(channel: channel,
                                                    messageConsumeDispatcher: _messageConsumeDispatcher,
                                                    objectSerializer: _objectSerializer,
                                                    messageTransmissionTypes: messageTransmissionTypes,
                                                    messageHandleOptions: messageHandleOptions,
                                                    targetEventNames: targetEventNames,
                                                    diagnosticSource: _diagnosticSource,
                                                    logger: _consumeLogger);

            await channel.BasicConsumeAsync(queue: targetQueue,
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

    /// <summary>
    /// 获取消息的传输类型字典和消息处理选项
    /// </summary>
    /// <param name="messageTransmissionTypes"></param>
    /// <param name="messageHandleOptions"></param>
    private void GetMessageOptions(out ImmutableDictionary<string, Type> messageTransmissionTypes, out ImmutableDictionary<string, MessageHandleOptions> messageHandleOptions)
    {
        Dictionary<string, Type> innerMessageTransmissionTypes = [];
        Dictionary<string, MessageHandleOptions> innerMessageHandleOptions = [];
        var defaultHandleOptions = new MessageHandleOptions()
        {
            RequeuePolicy = MessageRequeuePolicy.Unlimited,
            RequeueDelay = RabbitMQOptions.MessageRequeueDelay
        };
        foreach (var (eventName, eventInvokerDescriptors) in _eventSubscribeDescriptors)
        {
            var messageTransmissionType = eventInvokerDescriptors.GroupBy(static m => m.TransmissionType)
                                                                 .Select(static m => m.Key)
                                                                 .Single();
            innerMessageTransmissionTypes.Add(eventName, messageTransmissionType);
            var handleOptions = _options.MessageGroups.Where(m => m.Value.Messages.ContainsKey(eventName))
                                                      .SingleOrDefault().Value?.MessageHandleOptions
                                ?? defaultHandleOptions;
            innerMessageHandleOptions.Add(eventName, handleOptions);
        }
        messageTransmissionTypes = innerMessageTransmissionTypes.ToImmutableDictionary();
        messageHandleOptions = innerMessageHandleOptions.ToImmutableDictionary();
    }

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
