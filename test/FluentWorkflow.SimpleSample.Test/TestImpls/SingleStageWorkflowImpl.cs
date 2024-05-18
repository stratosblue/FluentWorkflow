using FluentWorkflow.Interface;
using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Message;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

internal class SingleStageWorkflowImpl : SingleStageWorkflow
{
    #region Private 字段

    private readonly IServiceScopeFactory _serviceScopeFactory;

    #endregion Private 字段

    #region Public 构造函数

    public SingleStageWorkflowImpl(SingleStageWorkflowContext context, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
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

    protected override Task OnSampleStage5Async(SingleStageWorkflowSampleStage5StageMessage message, MessageFireDelegate<SingleStageWorkflowSampleStage5StageMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage5Async(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage5CompletedAsync(SingleStageWorkflowSampleStage5StageCompletedMessage message, MessageFireDelegate<SingleStageWorkflowSampleStage5StageCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage5CompletedAsync(message, fireMessage, cancellationToken);
    }

    protected Task RunWithResumeAsync(IWorkflowContextCarrier<SingleStageWorkflowContext> message)
    {
        message.Context.SetBoolean("CurrentStageResumed", true);
        var bytes = SerializeContext(message.Context);

        Task.Run(async () =>
        {
            await Task.Yield();
            using var serviceScope = _serviceScopeFactory.CreateScope();
            await ResumeAsync(bytes, serviceScope.ServiceProvider, default);
        });
        return Task.CompletedTask;
    }

    protected bool ShouldWorkWithResume(SingleStageWorkflowContext context)
    {
        if (Context.WorkWithResume
            && context.GetBoolean("CurrentStageResumed", false) == false)
        {
            return true;
        }

        context.SetBoolean("CurrentStageResumed", false);

        return false;
    }

    #endregion Protected 方法
}
