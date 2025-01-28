#pragma warning disable CS1591

using FluentWorkflow;
using FluentWorkflow.SimpleSample;

[assembly: GenerateWorkflowCodes<AnotherWorkflowDeclaration>]
namespace FluentWorkflow.SimpleSample;

public partial class AnotherWorkflow
{
    public AnotherWorkflow(AnotherWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }
}

public partial class AnotherWorkflowDeclaration : IWorkflowDeclaration
{
    internal override void DeclareContext(IWorkflowContextDeclarator declarator)
    {
        declarator.Property<SampleWorkflowTestInfo>("TestInfo");
    }

    internal override void DeclareWorkflow(IWorkflowDeclarator declarator)
    {
        declarator.Name("Another")
                  .Begin()
                  .Then("SampleStage5")
                  .Then("SampleStage4")
                  .Then("SampleStage3")
                  .Then("SampleStage2")
                  .Then("SampleStage1")
                  .Completion();
    }
}
