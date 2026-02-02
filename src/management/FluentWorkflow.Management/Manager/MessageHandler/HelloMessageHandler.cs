using System.ComponentModel.RuntimeValidation;
using FluentWorkflow.Management.Shared;
using FluentWorkflow.Management.Shared.MessageHandler;
using FluentWorkflow.Management.Shared.Messages;
using Hoarwell;
using Hoarwell.Features;
using LazyProperties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentWorkflow.Management.Manager.MessageHandler;

[LazyProperty]
internal partial class HelloMessageHandler([ServiceKey] string appName,
                                           IOptionsMonitor<ManagementManagerOptions> optionsMonitor,
                                           IServiceProvider serviceProvider)
    : GenericResponseMessageHandler<Hello, Welcome>(appName, serviceProvider)
{
    #region Public 属性

    public partial ManagementManager ManagementManager { get; }

    #endregion Public 属性

    #region Protected 方法

    protected override async Task<Welcome> ProcessRequestAsync(IHoarwellContext context, Hello input, CancellationToken cancellationToken)
    {
        //TODO 协议版本处理
        //当前仅判断是否相同
        if (input.ProtocolVersion != SharedConstants.ProtocolVersion)
        {
            throw new AbortConnectionException($"Unsupported protocol version: {input.ProtocolVersion}");
        }
        if (string.Equals(input.Cookie.Required(), optionsMonitor.Get(AppName).Cookie.Required(), StringComparison.Ordinal))
        {
            if (input.Id == Guid.Empty)
            {
                throw new AbortConnectionException($"Invalid id from worker: {input.Id}");
            }

            var welcome = new Welcome()
            {
                Version = SharedConstants.Version,
            };

            var endPoint = context.RequiredFeature<IPipeEndPointFeature>().RemoteEndPoint;

            var workerDescriptor = new WorkerDescriptor(input.Id, input.WhoIs, input.HostName, input.Version, input.StartupTime, input.Metadata, endPoint);

            context.Features.Set(workerDescriptor);

            var workerContext = new WorkerContext(context, workerDescriptor)
            {
                CommunicationPipe = context.RequiredCommunicationPipe(),
            };

            await ManagementManager.WorkerConnectedAsync(workerContext);

            return welcome;
        }
        else
        {
            throw new AbortConnectionException($"Invalid cookie from worker: {input.Cookie}");
        }
    }

    #endregion Protected 方法
}
