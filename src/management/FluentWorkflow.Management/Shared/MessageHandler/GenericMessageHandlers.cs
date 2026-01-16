using System.ComponentModel.RuntimeValidation;
using FluentWorkflow.Management;
using FluentWorkflow.Management.Shared.Features;
using FluentWorkflow.Management.Shared.Messages.Abstractions;
using Hoarwell;
using Hoarwell.Features;

namespace FluentWorkflow.Management.Shared.MessageHandler;

/// <summary>
/// 泛型Ack消息处理委托
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="context"></param>
/// <param name="input"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task GenericAckMessageHandleDelegate<T>(IHoarwellContext context, AckMessage<T> input, CancellationToken cancellationToken);

/// <summary>
/// 泛型消息处理委托
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="context"></param>
/// <param name="input"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task GenericMessageHandleDelegate<T>(IHoarwellContext context, Message<T> input, CancellationToken cancellationToken);

/// <summary>
/// 泛型委托Ack消息处理器
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="callback"></param>
internal sealed class GenericDelegatingAckMessageHandler<T>(GenericAckMessageHandleDelegate<T> callback)
    : IEndpointMessageHandler<AckMessage<T>>
{
    #region Public 方法

    /// <inheritdoc/>
    public Task HandleAsync(IHoarwellContext context, AckMessage<T>? input)
    {
        return callback(context, input.Required(), context.ExecutionAborted);
    }

    #endregion Public 方法
}

/// <summary>
/// 泛型消息处理器
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class GenericMessageHandler<T>
    : IEndpointMessageHandler<AckMessage<T>>
{
    #region Public 方法

    /// <inheritdoc/>
    public Task HandleAsync(IHoarwellContext context, AckMessage<T>? input)
    {
        return context.Features.RequiredFeature<IMessageAckFeature>().ReceiveAckAsync<T>(input.Required(), context.ExecutionAborted);
    }

    #endregion Public 方法
}

/// <summary>
/// 泛型委托消息处理器
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="callback"></param>
internal sealed class GenericDelegatingMessageHandler<T>(GenericMessageHandleDelegate<T> callback)
    : IEndpointMessageHandler<Message<T>>
{
    #region Public 方法

    /// <inheritdoc/>
    public Task HandleAsync(IHoarwellContext context, Message<T>? input)
    {
        return callback(context, input.Required(), context.ExecutionAborted);
    }

    #endregion Public 方法
}
