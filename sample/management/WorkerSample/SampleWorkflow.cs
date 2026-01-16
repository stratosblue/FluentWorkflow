#pragma warning disable CS1591

using FluentWorkflow;
using FluentWorkflow.WorkerSample;
using FluentWorkflow.WorkerSample.SampleWorkflow.Message;
using Microsoft.Extensions.Logging;

[assembly: GenerateWorkflowCodes<SampleWorkflowDeclaration>]

namespace FluentWorkflow.WorkerSample;

public partial class SampleWorkflowWorkflow(SampleWorkflowWorkflowContext context, IServiceProvider serviceProvider)
    : SampleWorkflowWorkflowBase(context, serviceProvider)
{
}

internal partial class SampleWorkflowDeclaration : IWorkflowDeclaration
{
    #region Internal 方法

    internal override void DeclareContext(IWorkflowContextDeclarator declarator)
    {
        declarator.Property<TimeSpan>("Delay");
    }

    internal override void DeclareWorkflow(IWorkflowDeclarator declarator)
    {
        declarator.Name("SampleWorkflow")
                  .Begin()
                  .Then("Process")
                  .Completion();
    }

    #endregion Internal 方法
}

internal class StageProcessHandler(IServiceProvider serviceProvider, ILogger<StageProcessHandler> logger)
    : SampleWorkflow.Handler.StageProcessHandlerBase(serviceProvider)
{
    #region Protected 方法

    protected override async Task ProcessAsync(ProcessContext processContext, StageProcessMessage stageMessage, CancellationToken cancellationToken)
    {
        logger.LogInformation("Start process message {Id} with deley {Delay}.", stageMessage.Id, stageMessage.Context.Delay);
        await Task.Delay(stageMessage.Context.Delay, cancellationToken);
        logger.LogInformation("Message {Id} with deley {Delay} completed.", stageMessage.Id, stageMessage.Context.Delay);
    }

    #endregion Protected 方法
}
