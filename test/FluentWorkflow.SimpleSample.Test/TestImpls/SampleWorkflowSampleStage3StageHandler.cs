using FluentWorkflow.SimpleSample.Handler;
using FluentWorkflow.SimpleSample.Message;

namespace FluentWorkflow;

internal class SampleWorkflowSampleStage3StageHandler : SampleWorkflowSampleStage3StageHandlerBase
{
    #region Public 构造函数

    public SampleWorkflowSampleStage3StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override async Task ProcessAsync(ProcessContext processContext, SampleWorkflowSampleStage3StageMessage stageMessage, CancellationToken cancellationToken)
    {
        await SampleWorkflowSampleStage1StageHandler.StandardSampleWorkflowTestProcessAsync(stageMessage, ServiceProvider, processContext.AwaitChildWorkflow);
    }

    #endregion Protected 方法
}
