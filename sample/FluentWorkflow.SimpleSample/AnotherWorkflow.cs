#pragma warning disable CS1591

using FluentWorkflow.Interface;

namespace FluentWorkflow.SimpleSample;

public partial class AnotherWorkflow : IWorkflow
{
    public AnotherWorkflow(AnotherWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }

    protected override void BuildStages(IAnotherWorkflowStageBuilder stageBuilder)
    {
        stageBuilder.Begin()
                    .Then("SampleStage5")
                    .Then("SampleStage4")
                    .Then("SampleStage3")
                    .Then("SampleStage2")
                    .Then("SampleStage1")
                    .Completion();
    }
}
