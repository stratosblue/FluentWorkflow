using TemplateNamespace.Template.Handler;
using TemplateNamespace.Template.Message;

namespace FluentWorkflow;

internal class TemplateWorkflowStage2BPTGStageHandler : StageStage2BPTGHandlerBase
{
    #region Public 构造函数

    public TemplateWorkflowStage2BPTGStageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override async Task ProcessAsync(ProcessContext processContext, StageStage2BPTGMessage stageMessage, CancellationToken cancellationToken)
    {
        await TemplateWorkflowStage1CAUKStageHandler.StandardTemplateWorkflowTestProcessAsync(stageMessage, ServiceProvider, processContext.AwaitChildWorkflow);
    }

    #endregion Protected 方法
}
