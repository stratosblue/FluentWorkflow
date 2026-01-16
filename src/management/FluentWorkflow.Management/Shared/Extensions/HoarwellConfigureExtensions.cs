using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Management;
using FluentWorkflow.Management.Shared;
using FluentWorkflow.Management.Shared.Communication;
using FluentWorkflow.Management.Shared.MessageHandler;
using FluentWorkflow.Management.Shared.Messages.Abstractions;
using Hoarwell;
using Hoarwell.Build;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentWorkflow.Management.Shared;

internal static class HoarwellConfigureExtensions
{
    #region Internal 方法

    internal static HoarwellBuilder<HoarwellContext, Stream, Stream> ConfigureMessageSerialization(this HoarwellBuilder<HoarwellContext, Stream, Stream> builder)
    {
        builder.Services.TryAddKeyedSingleton<ITypeIdentifierAnalyzer, HoarwellTypeIdentifierAnalyzer>(builder.ApplicationName);

        builder.Services.TryAddKeyedSingleton<Hoarwell.IObjectSerializer, HoarwellObjectSerializer>(builder.ApplicationName);

        return builder;
    }

    internal static HoarwellEndpointBuilder GenericAck<TMessage>(this HoarwellEndpointBuilder builder) where TMessage : class
    {
        return builder.Handle<AckMessage<TMessage>, GenericMessageHandler<TMessage>>();
    }

    internal static HoarwellEndpointBuilder HandleMessage<TMessage, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMessageHander>(this HoarwellEndpointBuilder builder) where TMessage : class
        where TMessageHander : IEndpointMessageHandler<Message<TMessage>>
    {
        return builder.Handle<Message<TMessage>, TMessageHander>();
    }

    #region OnAckMessage

    internal static HoarwellEndpointBuilder OnAckMessage<TMessage>(this HoarwellEndpointBuilder builder, GenericAckMessageHandleDelegate<TMessage> callback) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(callback);
        return builder.Handle<AckMessage<TMessage>, GenericDelegatingAckMessageHandler<TMessage>>(new GenericDelegatingAckMessageHandler<TMessage>(callback));
    }

    internal static HoarwellEndpointBuilder OnAckMessage<TMessage>(this HoarwellEndpointBuilder builder, Func<IHoarwellContext, AckMessage<TMessage>, Task> callback) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(callback);
        return builder.Handle<AckMessage<TMessage>, GenericDelegatingAckMessageHandler<TMessage>>(new GenericDelegatingAckMessageHandler<TMessage>((context, message, _) => callback(context, message)));
    }

    internal static HoarwellEndpointBuilder OnAckMessage<TMessage>(this HoarwellEndpointBuilder builder, Func<AckMessage<TMessage>, Task> callback) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(callback);
        return builder.Handle<AckMessage<TMessage>, GenericDelegatingAckMessageHandler<TMessage>>(new GenericDelegatingAckMessageHandler<TMessage>((_, message, _) => callback(message)));
    }

    #endregion OnAckMessage

    #region OnMessage

    internal static HoarwellEndpointBuilder OnMessage<TMessage>(this HoarwellEndpointBuilder builder, GenericMessageHandleDelegate<TMessage> callback) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(callback);
        return builder.Handle<Message<TMessage>, GenericDelegatingMessageHandler<TMessage>>(new GenericDelegatingMessageHandler<TMessage>(callback));
    }

    internal static HoarwellEndpointBuilder OnMessage<TMessage>(this HoarwellEndpointBuilder builder, Func<IHoarwellContext, Message<TMessage>, Task> callb) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(callb);
        return builder.Handle<Message<TMessage>, GenericDelegatingMessageHandler<TMessage>>(new GenericDelegatingMessageHandler<TMessage>((context, message, _) => callb(context, message)));
    }

    internal static HoarwellEndpointBuilder OnMessage<TMessage>(this HoarwellEndpointBuilder builder, Func<Message<TMessage>, Task> callb) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(callb);
        return builder.Handle<Message<TMessage>, GenericDelegatingMessageHandler<TMessage>>(new GenericDelegatingMessageHandler<TMessage>((_, message, _) => callb(message)));
    }

    #endregion OnMessage

    #endregion Internal 方法
}
