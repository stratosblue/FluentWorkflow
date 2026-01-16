using FluentWorkflow.Management.Shared.Communication;
using Hoarwell;
using Hoarwell.Features;

namespace FluentWorkflow.Management;

internal static class InternalHoarwellContextExtensions
{
    extension(IHoarwellContext context)
    {
        public int NextMessageId()
        {
            return context.Features.RequiredFeature<IncreasingIdGenerator>().Next();
        }
    }
}
