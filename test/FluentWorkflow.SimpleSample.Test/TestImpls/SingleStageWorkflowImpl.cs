using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Message;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

internal class SingleStageWorkflowImpl : SingleStageWorkflow
{
    #region Public 构造函数

    public SingleStageWorkflowImpl(SingleStageWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task OnCompletionAsync(SingleStageWorkflowContext context, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetResult();
        return base.OnCompletionAsync(context, cancellationToken);
    }

    protected override Task OnFailedAsync(SingleStageWorkflowFailureMessage message, MessageFireDelegate<SingleStageWorkflowFailureMessage> fireMessage, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetException(new WorkflowFailureException(Id, message.Stage, message.Message, message.RemoteStackTrace, message.Context));
        return base.OnFailedAsync(message, fireMessage, cancellationToken);
    }

    #endregion Protected 方法
}
