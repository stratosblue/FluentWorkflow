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

public partial class SampleWorkflowDeclaration : IWorkflowDeclaration
{
    internal override void DeclareContext(IWorkflowContextDeclarator declarator)
    {
        declarator.Property<string>("ChildWorkflowStartStage")
                  .Property<int>("Depth")
                  .Property<int>("ExceptionDepth")
                  .Property<int>("ExceptionStep")
                  .Property<int>("MaxStageDelay")
                  .Property<int>("MaxSubWorkflow")
                  .Property<int>("Step")
                  .Property<int>("StepBase")
                  .Property<bool>("WorkWithResume", "通过 resume 工作，在每个阶段先挂起再恢复");
    }

    internal override void DeclareWorkflow(IWorkflowDeclarator declarator)
    {
        declarator.Name("Sample")
                  .Begin()
                  .Then("SampleStage1")
                  .Then("SampleStage2")
                  .Then("SampleStage3")
                  .Completion();
    }
}
