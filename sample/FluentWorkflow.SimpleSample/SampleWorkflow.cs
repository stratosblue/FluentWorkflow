#pragma warning disable CS1591

using FluentWorkflow.Interface;

namespace FluentWorkflow.SimpleSample;

public partial class SampleWorkflow : IWorkflow
{
    public SampleWorkflow(SampleWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }

    protected override void BuildStages(ISampleWorkflowStageBuilder stageBuilder)
    {
        stageBuilder.Begin()
                    .Then("SampleStage1")
                    .Then("SampleStage2")
                    .Then("SampleStage3")
                    .Completion();
    }
}
