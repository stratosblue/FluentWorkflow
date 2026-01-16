using FluentWorkflow.Management.Shared.Messages;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Manager.MessageHandler;

internal class PingPongMessageHandler([ServiceKey] string appName, IServiceProvider serviceProvider)
    : AuthorizeGenericResponseMessageHandler<Ping, Pong>(appName, serviceProvider)
{
    #region Protected 方法

    protected override Task<Pong> ProcessRequestAsync(IHoarwellContext context, Ping input, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Pong());
    }

    #endregion Protected 方法
}
