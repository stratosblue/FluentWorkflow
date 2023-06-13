#pragma warning disable CS1591

using FluentWorkflow.Interface;

namespace FluentWorkflow.SimpleSample;

public partial class SingleStageWorkflow : IWorkflow
{
    public SingleStageWorkflow(SingleStageWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }

    protected override void BuildStages(ISingleStageWorkflowStageBuilder stageBuilder)
    {
        stageBuilder.Begin()
                    .Then("SampleStage5")
                    .Completion();
    }
}
