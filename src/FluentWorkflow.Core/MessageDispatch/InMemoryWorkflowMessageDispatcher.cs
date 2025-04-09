using System.Collections.Immutable;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Build;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 基于内存的 <inheritdoc cref="IWorkflowMessageDispatcher"/>
/// </summary>
public class InMemoryWorkflowMessageDispatcher : IWorkflowMessageDispatcher
{
    #region Protected 字段

    /// <summary>
    /// 工作流程事件执行程序描述符订阅列表
    /// </summary>
    protected ImmutableDictionary<string, ImmutableArray<WorkflowEventInvokerDescriptor>> EventSubscribeDescriptors { get; }

    /// <inheritdoc cref="ILogger"/>
    protected ILogger Logger { get; }

    /// <inheritdoc cref="IObjectSerializer"/>
    protected IObjectSerializer ObjectSerializer { get; }

    /// <inheritdoc cref="IServiceScopeFactory"/>
    protected IServiceScopeFactory ServiceScopeFactory { get; }

    #endregion Protected 字段

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="InMemoryWorkflowMessageDispatcher"/>
    /// </summary>
    /// <param name="serviceScopeFactory"></param>
    /// <param name="workflowBuildStates"></param>
    /// <param name="objectSerializer"></param>
    /// <param name="logger"></param>
    public InMemoryWorkflowMessageDispatcher(IServiceScopeFactory serviceScopeFactory,
                                             WorkflowBuildStateCollection workflowBuildStates,
                                             IObjectSerializer objectSerializer,
                                             ILogger<InMemoryWorkflowMessageDispatcher> logger)
    {
        ArgumentNullException.ThrowIfNull(workflowBuildStates);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(objectSerializer);
        ArgumentNullException.ThrowIfNull(logger);

        ServiceScopeFactory = serviceScopeFactory;
        ObjectSerializer = objectSerializer;
        EventSubscribeDescriptors = workflowBuildStates.SelectMany(m => m)
                                                       .ToImmutableDictionary(m => m.EventName, m => m.ToImmutableArray());
        Logger = logger;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!EventSubscribeDescriptors.TryGetValue(TMessage.EventName, out var invokerDescriptors))
        {
            if (await OnNotSubscribedMessageAsync(message, cancellationToken))
            {
                return;
            }
            throw new InvalidOperationException($"Not found event subscriber for {{{message.Id}}}\"{TMessage.EventName}\".");
        }

        var messageJson = ObjectSerializer.Serialize(message);
        var messageClone = ObjectSerializer.Deserialize<TMessage>(messageJson)!;

        _ = Task.Run(async () =>
        {
            await Task.Yield();
            try
            {
                await using var serviceScope = ServiceScopeFactory.CreateAsyncScope();
                var serviceProvider = serviceScope.ServiceProvider;
                if (invokerDescriptors.Length == 1)
                {
                    var handler = serviceProvider.GetRequiredService(invokerDescriptors[0].TargetHandlerType);
                    await invokerDescriptors[0].HandlerInvokeDelegate(handler, messageClone, CancellationToken.None);
                }
                else
                {
                    var tasks = invokerDescriptors.Select(invokerDescriptor =>
                    {
                        var handler = serviceProvider.GetRequiredService(invokerDescriptor.TargetHandlerType);
                        return invokerDescriptor.HandlerInvokeDelegate(handler, messageClone, CancellationToken.None);
                    }).ToList();

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error at handle message - {{{Id}}}\"{EventName}\"", message.Id, TMessage.EventName);
            }
        }, cancellationToken);
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 在出现未订阅的消息时执行的方法
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回 <see langword="true"/> 则不抛出异常</returns>
    protected virtual Task<bool> OnNotSubscribedMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        return Task.FromResult(false);
    }

    #endregion Protected 方法
}
