#pragma warning disable CS1591

namespace FluentWorkflow.SharedSample;

public partial class SharedXXWorkflowDeclaration : IWorkflowDeclaration
{
    internal override void DeclareContext(IWorkflowContextDeclarator declarator)
    {
        declarator.Property<SharedXXWorkflowTestInfo>("TestInfo");
    }

    internal override void DeclareWorkflow(IWorkflowDeclarator declarator)
    {
        declarator.Name("SharedXX")
                  .Begin()
                  .Then("One")
                  .Then("Two")
                  .Then("Three")
                  .Completion();
    }
}
