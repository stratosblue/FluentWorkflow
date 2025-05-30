﻿#pragma warning disable CS1591

using FluentWorkflow;
using FluentWorkflow.Abstractions;
using FluentWorkflow.SimpleSample;

[assembly: GenerateWorkflowCodes<SingleStageWorkflowDeclaration>]
namespace FluentWorkflow.SimpleSample;

public partial class SingleStageWorkflow : IWorkflow
{
    public SingleStageWorkflow(SingleStageWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }
}

public partial class SingleStageWorkflowDeclaration : IWorkflowDeclaration
{
    internal override void DeclareContext(IWorkflowContextDeclarator declarator)
    {
        declarator.Property<SingleStageTestInfo>("TestInfo");
    }

    internal override void DeclareWorkflow(IWorkflowDeclarator declarator)
    {
        declarator.Name("SingleStage")
                  .Begin()
                  .Then("SampleStage5")
                  .Completion();
    }
}
