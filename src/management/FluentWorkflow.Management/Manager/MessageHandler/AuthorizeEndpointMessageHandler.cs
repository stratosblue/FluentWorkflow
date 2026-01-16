using System.ComponentModel.RuntimeValidation;
using FluentWorkflow.Management.Shared.Messages.Abstractions;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Manager.MessageHandler;

/// <summary>
/// 认证的泛型处理器
/// </summary>
/// <typeparam name="TRequest"></typeparam>
internal abstract class AuthorizeEndpointMessageHandler<TRequest>([ServiceKey] string appName, IServiceProvider serviceProvider)
    : IEndpointMessageHandler<Message<TRequest>>
{
    #region Public 属性

    public string AppName { get; } = appName;

    #endregion Public 属性

    #region Public 方法

    public Task HandleAsync(IHoarwellContext context, Message<TRequest>? input)
    {
        if (!context.IsAuthorized())
        {
            context.Abort("Not Authorized");
            return Task.CompletedTask;
        }

        var cancellationToken = context.ExecutionAborted;

        return ProcessRequestAsync(context, input.Required().Data, cancellationToken);
    }

    #endregion Public 方法

    #region Protected 方法

    protected T? GetKeyedService<T>() => serviceProvider.GetKeyedService<T>(AppName);

    protected T GetRequiredKeyedService<T>() where T : notnull => serviceProvider.GetRequiredKeyedService<T>(AppName);

    protected abstract Task ProcessRequestAsync(IHoarwellContext context, TRequest input, CancellationToken cancellationToken);

    #endregion Protected 方法
}
