using FluentWorkflow.Management.Shared.Communication;
using FluentWorkflow.Management.Shared.Features;
using Hoarwell;
using Hoarwell.Features;

namespace FluentWorkflow.Management.Shared;

internal static class InternalSharedHoarwellContextExtensions
{
    extension(IHoarwellContext context)
    {
        public IncreasingIdGenerator RequiredIdGenerator()
        {
            return context.Features.RequiredFeature<IncreasingIdGenerator>();
        }

        public ICommunicationPipeFeature RequiredCommunicationPipe()
        {
            return context.Features.RequiredFeature<ICommunicationPipeFeature>();
        }
    }
}
