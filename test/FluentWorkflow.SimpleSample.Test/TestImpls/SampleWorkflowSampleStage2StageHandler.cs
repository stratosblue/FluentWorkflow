using FluentWorkflow.SimpleSample.Sample.Handler;
using FluentWorkflow.SimpleSample.Sample.Message;

namespace FluentWorkflow;

internal class SampleWorkflowSampleStage2StageHandler : StageSampleStage2HandlerBase
{
    #region Public 构造函数

    public SampleWorkflowSampleStage2StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override async Task ProcessAsync(ProcessContext processContext, StageSampleStage2Message stageMessage, CancellationToken cancellationToken)
    {
        await SampleWorkflowSampleStage1StageHandler.StandardSampleWorkflowTestProcessAsync(stageMessage, ServiceProvider, processContext.AwaitChildWorkflow);
    }

    #endregion Protected 方法
}
