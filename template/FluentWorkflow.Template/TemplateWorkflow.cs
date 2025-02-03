using FluentWorkflow;
using FluentWorkflow.Abstractions;

[assembly: GenerateWorkflowCodes<TemplateNamespace.TemplateWorkflowDeclaration>(WorkflowSourceGenerationMode.All)]

namespace TemplateNamespace;

public partial class TemplateWorkflow : IWorkflow
{
    #region Public 构造函数

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <param name="serviceProvider"></param>
    public TemplateWorkflow(TemplateWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }

    #endregion Public 构造函数
}

/// <summary>
///
/// </summary>
public sealed partial class TemplateWorkflowDeclaration : IWorkflowDeclaration
{
    #region Internal 方法

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

    #endregion Internal 方法
}
