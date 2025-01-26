using FluentWorkflow;
using FluentWorkflow.Interface;

[assembly: GenerateWorkflowCodes<TemplateNamespace.TemplateWorkflowDeclaration>(WorkflowSourceGenerationMode.All)]
namespace TemplateNamespace;

public partial class TemplateWorkflow : IWorkflow
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="serviceProvider"></param>
    public TemplateWorkflow(TemplateWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }
}

/// <summary>
/// 
/// </summary>
public sealed partial class TemplateWorkflowDeclaration : IWorkflowDeclaration
{
    /// <inheritdoc/>
    internal override void DeclareContext(IWorkflowContextDeclarator declarator)
    {
        declarator.Property<int>("UserId", "注释UserId")
                  .Property<string>("Name", "注释Name")
                  .Property<int?>("Age")
                  .Property<string?>("Address")
                  .Property<TemplateWorkflowTestInfo?>("TestInfo");
    }

    /// <inheritdoc/>
    internal override void DeclareWorkflow(IWorkflowDeclarator declarator)
    {
        declarator.Name("Template")
                  .Begin()
                  .Then("Stage1CAUK")
                  .Then("Stage2BPTG")
                  .Then("Stage3AWBN")
                  .Completion();
    }
}
