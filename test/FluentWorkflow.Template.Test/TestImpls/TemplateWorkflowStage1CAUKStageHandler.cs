using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;
using TemplateNamespace;
using TemplateNamespace.Template.Handler;
using TemplateNamespace.Template.Message;

namespace FluentWorkflow;

internal class TemplateWorkflowStage1CAUKStageHandler : TemplateWorkflowStage1CAUKStageHandlerBase
{
    #region Public 构造函数

    public TemplateWorkflowStage1CAUKStageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Internal 方法

    internal static async Task StandardTemplateWorkflowTestProcessAsync(ITemplateWorkflowStageMessage stageMessage, IServiceProvider serviceProvider, Action<TemplateWorkflow> onChildWorkflowCreated)
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
                var subContext = new TemplateWorkflowContext(Guid.NewGuid().ToString())
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
                var subWorkflow = serviceProvider.GetRequiredService<IWorkflowBuilder<TemplateWorkflow>>().Build(subContext);
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

    protected override async Task ProcessAsync(ProcessContext processContext, TemplateWorkflowStage1CAUKStageMessage stageMessage, CancellationToken cancellationToken)
    {
        await StandardTemplateWorkflowTestProcessAsync(stageMessage, ServiceProvider, processContext.AwaitChildWorkflow);
    }

    #endregion Protected 方法
}
