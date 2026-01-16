using FluentWorkflow.Management.Shared.Messages;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Manager.MessageHandler;

internal class WorkerStatusReportMessageHandler([ServiceKey] string appName, IServiceProvider serviceProvider)
    : AuthorizeEndpointMessageHandler<WorkerStatusReport>(appName, serviceProvider)
{
    #region Public 方法

    protected override Task ProcessRequestAsync(IHoarwellContext context, WorkerStatusReport input, CancellationToken cancellationToken)
    {
        context.RequiredWorkerDescriptor().ProcessingCount = input.ProcessingCount;
        return Task.CompletedTask;
    }

    #endregion Public 方法
}
