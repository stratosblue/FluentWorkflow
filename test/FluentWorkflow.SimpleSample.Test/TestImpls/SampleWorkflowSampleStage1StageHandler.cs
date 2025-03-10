﻿using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Sample.Handler;
using FluentWorkflow.SimpleSample.Sample.Message;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

internal class SampleWorkflowSampleStage1StageHandler : StageSampleStage1HandlerBase
{
    #region Public 构造函数

    public SampleWorkflowSampleStage1StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Internal 方法

    internal static async Task StandardSampleWorkflowTestProcessAsync(ISampleStageMessage stageMessage, IServiceProvider serviceProvider, Action<SampleWorkflow> onChildWorkflowCreated)
    {
        serviceProvider.GetRequiredService<WorkflowExecuteLogger>().Step(stageMessage);

        var context = stageMessage.Context.TestInfo!;
        if (context.Depth > 0
            && context.Step-- == 0)
        {
            context.Depth--;

            var count = context.MaxSubWorkflow > 0 ? context.MaxSubWorkflow : 1;

            var nextExceptionDepth = context.ExceptionDepth - 1;
            for (int i = 0; i < count; i++)
            {
                var subContext = new SampleWorkflowContext(Guid.NewGuid().ToString())
                {
                    TestInfo = new()
                    {
                        Depth = context.Depth,
                        StepBase = context.StepBase,
                        Step = context.StepBase,
                        ExceptionDepth = nextExceptionDepth,
                        ExceptionStep = context.ExceptionStep,
                    }
                };
                var subWorkflow = serviceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>().Build(subContext);
                onChildWorkflowCreated(subWorkflow);
            }
        }

        if (context.ExceptionStep == 1
            && context.ExceptionDepth <= 0)
        {
            throw new Exception("【Workflow Exception Threw.---------------------------------】");
        }

        if (context.ExceptionStep > 1)
        {
            context.ExceptionStep--;
        }

        if (context.MaxStageDelay > 0)
        {
            await Task.Delay(Random.Shared.Next(context.MaxStageDelay));
        }
    }

    #endregion Internal 方法

    #region Protected 方法

    protected override async Task ProcessAsync(ProcessContext processContext, StageSampleStage1Message stageMessage, CancellationToken cancellationToken)
    {
        await StandardSampleWorkflowTestProcessAsync(stageMessage, ServiceProvider, processContext.AwaitChildWorkflow);
    }

    #endregion Protected 方法
}
