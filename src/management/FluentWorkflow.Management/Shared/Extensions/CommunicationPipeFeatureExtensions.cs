using System.ComponentModel.RuntimeValidation;
using FluentWorkflow.Management.Shared.Features;
using FluentWorkflow.Management.Shared.Messages;

namespace FluentWorkflow.Management.Shared;

internal static class CommunicationPipeFeatureExtensions
{
    public static Task<Pong> PingAsync(this ICommunicationPipeFeature communicationPipeFeature, CancellationToken cancellationToken)
    {
        return communicationPipeFeature.RequestAsync<Ping, Pong>(new Ping(), cancellationToken).RequiredResult();
    }
}
