using System.Collections.Immutable;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 消息消费组构建器
/// </summary>
/// <param name="groupName"></param>
/// <param name="messageUniquenessCheckAction"></param>
public class MessageConsumeGroupBuilder(string groupName, Action<string, Type> messageUniquenessCheckAction)
{
    #region Private 字段

    private readonly MessageHandleOptions _messageHandleOptions = new();

    private readonly Dictionary<string, Type> _messages = [];

    private MessageConsumeGroupInitializationDelegate? _groupInitializationCallback;

    #endregion Private 字段

    #region Public 方法

    /// <summary>
    /// 添加消息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public MessageConsumeGroupBuilder Add<TMessage>()
        where TMessage : IWorkflowMessage, IEventNameDeclaration
    {
        messageUniquenessCheckAction(TMessage.EventName, typeof(TMessage));
        _messages[TMessage.EventName] = typeof(TMessage);
        return this;
    }

    /// <summary>
    /// 构建消息组
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MessageConsumeGroup Build()
    {
        if (_groupInitializationCallback is null)
        {
            throw new InvalidOperationException("Must set initialization callback first.");
        }
        return new MessageConsumeGroup(groupName, _messages.ToImmutableDictionary(), _groupInitializationCallback, _messageHandleOptions);
    }

    /// <summary>
    /// 配置选项
    /// </summary>
    /// <param name="setupAction"></param>
    public MessageConsumeGroupBuilder Option(Action<MessageHandleOptions> setupAction)
    {
        setupAction?.Invoke(_messageHandleOptions);
        return this;
    }

    /// <summary>
    /// 设置初始化回调
    /// </summary>
    /// <param name="groupInitializationCallback"></param>
    /// <returns></returns>
    public MessageConsumeGroupBuilder WithInitialization(MessageConsumeGroupInitializationDelegate groupInitializationCallback)
    {
        ArgumentNullException.ThrowIfNull(groupInitializationCallback);

        _groupInitializationCallback = groupInitializationCallback;
        return this;
    }

    #endregion Public 方法
}
