using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Message;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

internal class SampleWorkflowImpl : SampleWorkflow
{
    #region Public 构造函数

    public SampleWorkflowImpl(SampleWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task OnCompletionAsync(SampleWorkflowContext context, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetResult();
        return base.OnCompletionAsync(context, cancellationToken);
    }

    protected override Task OnFailedAsync(SampleWorkflowFailureMessage message, MessageFireDelegate<SampleWorkflowFailureMessage> fireMessage, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetException(new WorkflowFailureException(Id, message.Stage, message.Message, message.RemoteStackTrace, message.Context));
        return base.OnFailedAsync(message, fireMessage, cancellationToken);
    }

    #endregion Protected 方法
}
