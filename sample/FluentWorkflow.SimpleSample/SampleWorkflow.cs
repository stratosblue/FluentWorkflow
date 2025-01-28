#pragma warning disable CS1591

using FluentWorkflow;
using FluentWorkflow.SimpleSample;

[assembly: GenerateWorkflowCodes<SampleWorkflowDeclaration>]
namespace FluentWorkflow.SimpleSample;

public partial class SampleWorkflow
{
    public SampleWorkflow(SampleWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }
}

public partial class SampleWorkflowDeclaration : IWorkflowDeclaration
{
    internal override void DeclareContext(IWorkflowContextDeclarator declarator)
    {
        declarator.Property<SampleWorkflowTestInfo>("TestInfo");
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
