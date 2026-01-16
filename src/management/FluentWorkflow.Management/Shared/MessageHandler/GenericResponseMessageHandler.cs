using System.ComponentModel.RuntimeValidation;
using FluentWorkflow.Management;
using FluentWorkflow.Management.Shared.Messages.Abstractions;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Shared.MessageHandler;

internal abstract class GenericResponseMessageHandler<TRequest, TResponse>([ServiceKey] string appName, IServiceProvider serviceProvider)
    : IEndpointMessageHandler<Message<TRequest>>
{
    #region Public 属性

    public string AppName { get; } = appName;

    #endregion Public 属性

    #region Public 方法

    public virtual async Task HandleAsync(IHoarwellContext context, Message<TRequest>? input)
    {
        input.Required();

        var cancellationToken = context.ExecutionAborted;

        TResponse? response = default;
        var isExecutionSuccess = true;
        string? executionMessage = null;

        try
        {
            response = await ProcessRequestAsync(context, input.Data, cancellationToken);
        }
        catch (AbortConnectionException) { throw; }
        catch (Exception ex)
        {
            isExecutionSuccess = false;
            executionMessage = $"Error: {ex.Message}";
        }

        var ackMessage = new AckMessage<TResponse>()
        {
            Id = context.NextMessageId(),
            AckId = input.Id,
            Data = response,
            IsExecutionSuccess = isExecutionSuccess,
            ExecutionMessage = executionMessage,
        };

        await context.WriteAndFlushAsync(ackMessage, cancellationToken);
    }

    #endregion Public 方法

    #region Protected 方法

    protected T? GetKeyedService<T>() => serviceProvider.GetKeyedService<T>(AppName);

    protected T GetRequiredKeyedService<T>() where T : notnull => serviceProvider.GetRequiredKeyedService<T>(AppName);

    protected abstract Task<TResponse> ProcessRequestAsync(IHoarwellContext context, TRequest input, CancellationToken cancellationToken);

    #endregion Protected 方法
}
