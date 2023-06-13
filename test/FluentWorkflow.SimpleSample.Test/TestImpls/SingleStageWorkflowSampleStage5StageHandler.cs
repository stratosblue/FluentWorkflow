using FluentWorkflow.Interface;
using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Handler;
using FluentWorkflow.SimpleSample.Message;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

internal class SingleStageWorkflowSampleStage5StageHandler : SingleStageWorkflowSampleStage5StageHandlerBase
{
    #region Public 构造函数

    public SingleStageWorkflowSampleStage5StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override async Task ProcessAsync(ProcessContext processContext, SingleStageWorkflowSampleStage5StageMessage stageMessage, CancellationToken cancellationToken)
    {
        ServiceProvider.GetRequiredService<WorkflowExecuteLogger>().Step(stageMessage);

        var context = stageMessage.Context;
        if (context.Loop > 0
            && context.Step-- == 0)
        {
            context.Loop--;

            var count = context.MaxSubWorkflow > 0 ? context.MaxSubWorkflow : 1;

            for (int i = 0; i < count; i++)
            {
                var subContext = new SampleWorkflowContext(Guid.NewGuid().ToString())
                {
                    Depth = context.Loop,
                    StepBase = context.StepBase,
                    Step = context.StepBase,
                };
                var subWorkflow = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>().Build(subContext);
                processContext.AwaitChildWorkflow(subWorkflow);
            }
        }

        if (context.ExceptionStep == 1)
        {
            throw new Exception("Exception Threw.---------------------------------");
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

    #endregion Protected 方法
}
