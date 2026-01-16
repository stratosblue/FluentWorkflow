using Hoarwell;
using Hoarwell.Features;

namespace FluentWorkflow.Management.Worker;

internal static class InternalWorkerHoarwellContextExtensions
{
    extension(IHoarwellContext context)
    {
        public ManagementClusterConnector RequiredManagementWorker()
        {
            return context.Features.RequiredFeature<ManagementClusterConnector>();
        }
    }
}
