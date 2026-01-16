using FluentWorkflow.Management.Shared.MessageHandler;
using FluentWorkflow.Management.Shared.Messages.Abstractions;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Manager.MessageHandler;

/// <summary>
/// 认证的泛型响应处理器
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
internal abstract class AuthorizeGenericResponseMessageHandler<TRequest, TResponse>([ServiceKey] string appName, IServiceProvider serviceProvider)
    : GenericResponseMessageHandler<TRequest, TResponse>(appName, serviceProvider)
{
    #region Public 方法

    public override async Task HandleAsync(IHoarwellContext context, Message<TRequest>? input)
    {
        if (!context.IsAuthorized())
        {
            context.Abort("Not Authorized");
            return;
        }
        await base.HandleAsync(context, input);
    }

    #endregion Public 方法
}
