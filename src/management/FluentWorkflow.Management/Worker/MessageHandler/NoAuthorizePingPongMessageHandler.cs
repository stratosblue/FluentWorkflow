using FluentWorkflow.Management.Shared.MessageHandler;
using FluentWorkflow.Management.Shared.Messages;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Worker.MessageHandler;

internal class NoAuthorizePingPongMessageHandler([ServiceKey] string appName, IServiceProvider serviceProvider)
    : GenericResponseMessageHandler<Ping, Pong>(appName, serviceProvider)
{
    #region Protected 方法

    protected override Task<Pong> ProcessRequestAsync(IHoarwellContext context, Ping input, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Pong());
    }

    #endregion Protected 方法
}
