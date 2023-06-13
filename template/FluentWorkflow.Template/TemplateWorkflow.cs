using FluentWorkflow.Interface;

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

    /// <inheritdoc/>
    protected override void BuildStages(ITemplateWorkflowStageBuilder stageBuilder)
    {
        stageBuilder.Begin()
                    .Then("Stage1CAUK")
                    .Then("Stage2BPTG")
                    .Then("Stage3AWBN")
                    .Completion();
    }
}
