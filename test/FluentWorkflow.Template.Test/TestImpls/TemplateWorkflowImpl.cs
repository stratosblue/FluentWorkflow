using Microsoft.Extensions.DependencyInjection;
using TemplateNamespace;
using TemplateNamespace.Message;

namespace FluentWorkflow;

internal class TemplateWorkflowImpl : TemplateWorkflow
{
    #region Public 构造函数

    public TemplateWorkflowImpl(TemplateWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task OnCompletionAsync(TemplateWorkflowContext context, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetResult();
        return base.OnCompletionAsync(context, cancellationToken);
    }

    protected override Task OnFailedAsync(TemplateWorkflowFailureMessage message, MessageFireDelegate<TemplateWorkflowFailureMessage> fireMessage, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetException(new WorkflowFailureException(Id, message.Stage, message.Message, message.RemoteStackTrace, message.Context));
        return base.OnFailedAsync(message, fireMessage, cancellationToken);
    }

    #endregion Protected 方法
}
