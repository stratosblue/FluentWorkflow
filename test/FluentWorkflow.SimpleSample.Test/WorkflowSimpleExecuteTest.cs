using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Sample;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

[TestClass]
public abstract class WorkflowSimpleExecuteTest : FluentWorkflowTestBase
{
    #region Public 方法

    [DataRow(3)]
    [DataRow(2)]
    [DataRow(1)]
    [TestMethod]
    public async Task Should_Break_With_Exception(int exceptionStep)
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new SampleWorkflowContext(id)
        {
            TestInfo = new()
            {
                ExceptionStep = exceptionStep,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await Assert.ThrowsExactlyAsync<WorkflowFailureException>(() => FinishWaiterContainer[id].WaitAsync());

        Assert.HasCount(exceptionStep, executeLogger.Stages);
    }

    [TestMethod]
    public async Task Should_Run_Success()
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new SampleWorkflowContext(id)
        {
            TestInfo = new()
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await FinishWaiterContainer[id].WaitAsync();

        Assert.HasCount(SampleStages.OrderedStageIds.Length, executeLogger.Stages);

        for (int i = 0; i < executeLogger.Stages.Count; i++)
        {
            Assert.AreEqual(SampleStages.OrderedStageIds[i], executeLogger.Stages[i].Stage);
        }
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddFluentWorkflow()
                .AddSampleWorkflow<SampleWorkflowImpl>(configuration =>
                {
                    configuration.AddScheduler()
                                 .AddResultObserver()
                                 .AddStageSampleStage1Handler<SampleWorkflowSampleStage1StageHandler>()
                                 .AddStageSampleStage2Handler<SampleWorkflowSampleStage2StageHandler>()
                                 .AddStageSampleStage3Handler<SampleWorkflowSampleStage3StageHandler>();
                });


        services.AddSingleton<WorkflowExecuteLogger>();
    }

    #endregion Protected 方法
}
