﻿using FluentWorkflow.Interface;
using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Message;
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

    protected override Task OnFailedAsync(SampleWorkflowFailureMessage message, MessageFireDelegate<SampleWorkflowFailureMessage> fireMessage, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<InMemoryWorkflowFinishWaiterContainer>()[Id].SetException(new WorkflowFailureException(Id, message.Stage, message.Message, message.RemoteStackTrace, message.Context));
        return base.OnFailedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage1Async(SampleWorkflowSampleStage1StageMessage message, MessageFireDelegate<SampleWorkflowSampleStage1StageMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage1Async(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage1CompletedAsync(SampleWorkflowSampleStage1StageCompletedMessage message, MessageFireDelegate<SampleWorkflowSampleStage1StageCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage1CompletedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage2Async(SampleWorkflowSampleStage2StageMessage message, MessageFireDelegate<SampleWorkflowSampleStage2StageMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage2Async(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage2CompletedAsync(SampleWorkflowSampleStage2StageCompletedMessage message, MessageFireDelegate<SampleWorkflowSampleStage2StageCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage2CompletedAsync(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage3Async(SampleWorkflowSampleStage3StageMessage message, MessageFireDelegate<SampleWorkflowSampleStage3StageMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage3Async(message, fireMessage, cancellationToken);
    }

    protected override Task OnSampleStage3CompletedAsync(SampleWorkflowSampleStage3StageCompletedMessage message, MessageFireDelegate<SampleWorkflowSampleStage3StageCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        if (ShouldWorkWithResume(message.Context))
        {
            return RunWithResumeAsync(message);
        }

        return base.OnSampleStage3CompletedAsync(message, fireMessage, cancellationToken);
    }

    protected Task RunWithResumeAsync(IWorkflowContextCarrier<SampleWorkflowContext> message)
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

    protected bool ShouldWorkWithResume(SampleWorkflowContext context)
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
