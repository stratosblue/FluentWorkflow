using System.Collections.Concurrent;
using FluentWorkflow.Management;
using FluentWorkflow.Management.Shared;
using FluentWorkflow.Management.Shared.Features;
using FluentWorkflow.Management.Shared.Messages.Abstractions;
using Hoarwell;

namespace FluentWorkflow.Management.Shared;

/// <summary>
/// 通信管道
/// </summary>
/// <param name="context"></param>
public sealed class CommunicationPipe(IHoarwellContext context)
    : ICommunicationPipeFeature, IMessageAckFeature, IAsyncDisposable
{
    #region Private 属性

    /// <summary>
    /// 等待中的消息列表
    /// </summary>
    private ConcurrentDictionary<int, TaskCompletionSource<object?>> WaitingMessages { get; } = new();

    #endregion Private 属性

    #region Public 方法

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        var exception = new ObjectDisposedException(GetType().Name);
        foreach (var message in WaitingMessages.Values)
        {
            message.TrySetException(exception);
        }
        WaitingMessages.Clear();

        return default;
    }

    /// <inheritdoc/>
    public Task ReceiveAckAsync<T>(AckMessage<T> message, CancellationToken cancellationToken)
    {
        if (!WaitingMessages.TryGetValue(message.AckId, out var completion))
        {
            throw new InvalidOperationException($"Receive ack message for \"{message.AckId}\". But there is no corresponding waiting here");
        }

        completion.TrySetResult(message);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<TRespone?> RequestAsync<TRequest, TRespone>(TRequest request, CancellationToken cancellationToken)
    {
        var message = new Message<TRequest>()
        {
            Id = context.NextMessageId(),
            Data = request,
        };

        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        WaitingMessages.TryAdd(message.Id, tcs);

        using var localCts = CancellationTokenSource.CreateLinkedTokenSource(context.ExecutionAborted, cancellationToken);
        cancellationToken = localCts.Token;

        try
        {
            await InnerSendAsync(message, cancellationToken);

            var responseMessage = await tcs.Task.WaitAsync(cancellationToken);

            if (responseMessage is AckMessage<TRespone> typedResponseMessage)
            {
                if (!typedResponseMessage.IsExecutionSuccess)
                {
                    throw new RemoteExecutionException(typedResponseMessage.ExecutionMessage);
                }
                return typedResponseMessage.Data;
            }

            if (responseMessage is not null)
            {
                throw new ErrorTargetMessageException(message, typeof(TRespone), responseMessage);
            }

            return default;
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
            throw;
        }
        finally
        {
            WaitingMessages.TryRemove(message.Id, out _);
        }
    }

    /// <inheritdoc/>
    public async Task ResponeAsync<TRespone>(int ackMessageId, TRespone response, CancellationToken cancellationToken)
    {
        using var localCts = CancellationTokenSource.CreateLinkedTokenSource(context.ExecutionAborted, cancellationToken);
        cancellationToken = localCts.Token;

        await InnerSendAsync(response, cancellationToken);
    }

    /// <inheritdoc/>
    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        var message = new Message<TRequest>()
        {
            Id = context.NextMessageId(),
            Data = request,
        };
        return InnerSendAsync(message, cancellationToken);
    }

    #endregion Public 方法

    #region Private 方法

    private Task InnerSendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        return context.WriteAndFlushAsync(request, cancellationToken);
    }

    #endregion Private 方法
}
