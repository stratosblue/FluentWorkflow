using FluentWorkflow.SimpleSample.Handler;
using FluentWorkflow.SimpleSample.Message;

namespace FluentWorkflow;

internal class SampleWorkflowSampleStage2StageHandler : SampleWorkflowSampleStage2StageHandlerBase
{
    #region Public 构造函数

    public SampleWorkflowSampleStage2StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override async Task ProcessAsync(ProcessContext processContext, SampleWorkflowSampleStage2StageMessage stageMessage, CancellationToken cancellationToken)
    {
        await SampleWorkflowSampleStage1StageHandler.StandardSampleWorkflowTestProcessAsync(stageMessage, ServiceProvider, processContext.AwaitChildWorkflow);
    }

    #endregion Protected 方法
}
