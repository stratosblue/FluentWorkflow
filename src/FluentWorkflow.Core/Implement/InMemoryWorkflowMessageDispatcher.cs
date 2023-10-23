using System.Collections.Immutable;
using System.Text.Json;
using FluentWorkflow.Build;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow;

internal class InMemoryWorkflowMessageDispatcher : IWorkflowMessageDispatcher
{
    #region Private 字段

    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly ImmutableDictionary<string, WorkflowEventInvokerDescriptor[]> _eventSubscribeDescriptors;

    private readonly ILogger _logger;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    #endregion Private 字段

    #region Public 构造函数

    public InMemoryWorkflowMessageDispatcher(IServiceScopeFactory serviceScopeFactory,
                                             WorkflowBuildStateCollection workflowBuildStates,
                                             ILogger<InMemoryWorkflowMessageDispatcher> logger)
    {
        if (workflowBuildStates is null)
        {
            throw new ArgumentNullException(nameof(workflowBuildStates));
        }

        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _eventSubscribeDescriptors = workflowBuildStates.SelectMany(m => m)
                                                        .ToImmutableDictionary(m => m.EventName, m => m.ToArray());
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    Task IWorkflowMessageDispatcher.PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (!_eventSubscribeDescriptors.TryGetValue(TMessage.EventName, out var invokerDescriptors))
        {
            throw new InvalidOperationException($"Not found event subscriber for {{{message.Id}}}\"{TMessage.EventName}\".");
        }

        var messageJson = JsonSerializer.Serialize(message, s_jsonSerializerOptions);
        var messageClone = JsonSerializer.Deserialize<TMessage>(messageJson, s_jsonSerializerOptions)!;

        _ = Task.Run(async () =>
        {
            try
            {
                await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
                var serviceProvider = serviceScope.ServiceProvider;
                if (invokerDescriptors.Length == 1)
                {
                    var handler = serviceProvider.GetRequiredService(invokerDescriptors[0].TargetType);
                    await invokerDescriptors[0].HandlerInvokeDelegate(handler, messageClone, CancellationToken.None);
                }
                else
                {
                    var tasks = invokerDescriptors.Select(invokerDescriptor =>
                    {
                        var handler = serviceProvider.GetRequiredService(invokerDescriptor.TargetType);
                        return invokerDescriptor.HandlerInvokeDelegate(handler, messageClone, CancellationToken.None);
                    }).ToArray();

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at handle message - {{{Id}}}\"{EventName}\"", message.Id, TMessage.EventName);
            }
        }, CancellationToken.None);

        return Task.CompletedTask;
    }

    #endregion Public 方法
}
