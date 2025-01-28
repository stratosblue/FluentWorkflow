using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;
using TemplateNamespace;
using TemplateNamespace.Template.Message;

namespace FluentWorkflow;

internal class TemplateWorkflowImpl : TemplateWorkflow
{
    #region Private 字段

    private readonly IServiceScopeFactory _serviceScopeFactory;

    #endregion Private 字段

    #region Public 构造函数

    public TemplateWorkflowImpl(TemplateWorkflowContext context, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task OnCompletionAsync(TemplateWorkflowContext context, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetResult();
        return base.OnCompletionAsync(context, cancellationToken);
    }

    protected override Task OnFailedAsync(TemplateFailureMessage message, MessageFireDelegate<TemplateFailureMessage> fireMessage, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetException(new WorkflowFailureException(Id, message.Stage, message.Message, message.RemoteStackTrace, message.Context));
        return base.OnFailedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnStage1CAUKAsync(StageStage1CAUKMessage message, MessageFireDelegate<StageStage1CAUKMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnStage1CAUKAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnStage1CAUKCompletedAsync(StageStage1CAUKCompletedMessage message, MessageFireDelegate<StageStage1CAUKCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnStage1CAUKCompletedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnStage2BPTGAsync(StageStage2BPTGMessage message, MessageFireDelegate<StageStage2BPTGMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnStage2BPTGAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnStage2BPTGCompletedAsync(StageStage2BPTGCompletedMessage message, MessageFireDelegate<StageStage2BPTGCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnStage2BPTGCompletedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnStage3AWBNAsync(StageStage3AWBNMessage message, MessageFireDelegate<StageStage3AWBNMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnStage3AWBNAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnStage3AWBNCompletedAsync(StageStage3AWBNCompletedMessage message, MessageFireDelegate<StageStage3AWBNCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnStage3AWBNCompletedAsync(message, fireMessage, cancellationToken);
    }

    protected Task RunWithResumeAsync(IWorkflowContextCarrier<TemplateWorkflowContext> message)
    {
        message.Context.SetValue("CurrentStageResumed", true);
        var bytes = SerializeContext(message.Context);

        Task.Run(async () =>
        {
            await Task.Yield();
            using var serviceScope = _serviceScopeFactory.CreateScope();
            await ResumeAsync(bytes, serviceScope.ServiceProvider, default);
        });
        return Task.CompletedTask;
    }

    protected bool ShouldWorkWithResume(TemplateWorkflowContext context)
    {
        if (Context.TestInfo?.WorkWithResume == true
            && context.GetValue<bool>("CurrentStageResumed") == false)
        {
            return true;
        }

        context.SetValue("CurrentStageResumed", false);

        return false;
    }

    #endregion Protected 方法
}
