using System.Diagnostics;
using FluentWorkflow.Management.Shared;
using FluentWorkflow.Management.Shared.Communication;
using FluentWorkflow.Management.Shared.Features;
using FluentWorkflow.Management.Shared.Messages;
using Hoarwell;
using Hoarwell.ExecutionPipeline;
using Hoarwell.Middlewares;

namespace FluentWorkflow.Management.Manager.Communication;

internal class ManagerInboundPipelineConfigureMiddleware : PassthroughMiddleware<HoarwellContext, Stream>
{
    #region Public 方法

    public override async Task InvokeAsync(HoarwellContext context, Stream input, PipelineInvokeDelegate<HoarwellContext, Stream> next)
    {
        using var registration = context.ExecutionAborting.Register(static async state =>
        {
            try
            {
                var context = (HoarwellContext)state!;
                var cancellationToken = context.ExecutionAborted;

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var close = new Close()
                {
                    Reason = context.CloseReason?.ToString() ?? "The server actively closes the connection.",
                };

                var sendTask = context.RequiredCommunicationPipe().SendAsync(close, cancellationToken);
                Task.WhenAny(sendTask, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken))
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                //忽略此处的异常

                Debug.WriteLine($"Context close callback failed: {ex}");
            }
        }, context);

        context.Features.Set(new IncreasingIdGenerator(2, 2));

        var communicationPipe = new CommunicationPipe(context);
        context.Features.Set<ICommunicationPipeFeature>(communicationPipe);
        context.Features.Set<IMessageAckFeature>(communicationPipe);

        await next(context, input);
    }

    #endregion Public 方法
}
