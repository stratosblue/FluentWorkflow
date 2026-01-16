using FluentWorkflow.Management;
using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Shared;
using Hoarwell;
using Hoarwell.Features;

namespace Hoarwell;

internal static class InternalManagerHoarwellContextExtensions
{
    extension(IHoarwellContext context)
    {
        public WorkerDescriptor RequiredWorkerDescriptor()
        {
            return context.Features.RequiredFeature<WorkerDescriptor>();
        }

        public bool IsAuthorized()
        {
            return context.Features.Get<WorkerDescriptor>() is not null;
        }
    }
}
