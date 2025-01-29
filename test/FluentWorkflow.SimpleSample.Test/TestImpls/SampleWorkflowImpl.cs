using FluentWorkflow.Interface;
using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Sample.Message;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

internal class SampleWorkflowImpl : SampleWorkflow
{
    #region Private 字段

    private readonly IServiceScopeFactory _serviceScopeFactory;

    #endregion Private 字段

    #region Public 构造函数

    public SampleWorkflowImpl(SampleWorkflowContext context, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task OnCompletionAsync(SampleWorkflowContext context, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetResult();
        return base.OnCompletionAsync(context, cancellationToken);
    }

    protected override Task OnFailedAsync(SampleFailureMessage message, MessageFireDelegate<SampleFailureMessage> fireMessage, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetException(new WorkflowFailureException(Id, message.Stage, message.Message, message.RemoteStackTrace, message.Context));
        return base.OnFailedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage1Async(StageSampleStage1Message message, MessageFireDelegate<StageSampleStage1Message> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage1Async(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage1CompletedAsync(StageSampleStage1CompletedMessage message, MessageFireDelegate<StageSampleStage1CompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage1CompletedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage2Async(StageSampleStage2Message message, MessageFireDelegate<StageSampleStage2Message> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage2Async(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage2CompletedAsync(StageSampleStage2CompletedMessage message, MessageFireDelegate<StageSampleStage2CompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage2CompletedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage3Async(StageSampleStage3Message message, MessageFireDelegate<StageSampleStage3Message> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage3Async(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage3CompletedAsync(StageSampleStage3CompletedMessage message, MessageFireDelegate<StageSampleStage3CompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage3CompletedAsync(message, fireMessage, cancellationToken);
    }

    protected Task RunWithResumeAsync(IWorkflowContextCarrier<SampleWorkflowContext> message)
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

    protected bool ShouldWorkWithResume(SampleWorkflowContext context)
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
