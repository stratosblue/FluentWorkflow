using TemplateNamespace;
using TemplateNamespace.Handler;
using TemplateNamespace.Message;

namespace FluentWorkflow;

internal class TemplateWorkflowStage3AWBNStageHandler : TemplateWorkflowStage3AWBNStageHandlerBase
{
    #region Public 构造函数

    public TemplateWorkflowStage3AWBNStageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override async Task ProcessAsync(ProcessContext processContext, TemplateWorkflowStage3AWBNStageMessage stageMessage, CancellationToken cancellationToken)
    {
        await TemplateWorkflowStage1CAUKStageHandler.StandardTemplateWorkflowTestProcessAsync(stageMessage, ServiceProvider, processContext.AwaitChildWorkflow);
    }

    #endregion Protected 方法
}
