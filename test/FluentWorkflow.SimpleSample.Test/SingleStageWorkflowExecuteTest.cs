using FluentWorkflow.Interface;
using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.SingleStage;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

[TestClass]
public abstract class SingleStageWorkflowExecuteTest : FluentWorkflowTestBase
{
    #region Protected 属性

    /// <inheritdoc cref="SingleStageWorkflowContext.WorkWithResume"/>
    protected virtual bool WorkWithResume { get; } = false;

    #endregion Protected 属性

    #region Public 方法

    [DataRow(1)]
    [TestMethod]
    public async Task Should_Break_With_Exception(int exceptionStep)
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SingleStageWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new SingleStageWorkflowContext(id)
        {
            TestInfo = new()
            {
                ExceptionStep = exceptionStep,
                WorkWithResume = WorkWithResume,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await Assert.ThrowsExceptionAsync<WorkflowFailureException>(() => FinishWaiterContainer[id].WaitAsync());

        Assert.AreEqual(exceptionStep, executeLogger.Stages.Count);
    }

    [TestMethod]
    public async Task Should_Run_Success()
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SingleStageWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new SingleStageWorkflowContext(id)
        {
            TestInfo = new()
            {
                WorkWithResume = WorkWithResume,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await FinishWaiterContainer[id].WaitAsync();

        Assert.AreEqual(SingleStageStages.OrderedStageIds.Length, executeLogger.Stages.Count);

        for (int i = 0; i < executeLogger.Stages.Count; i++)
        {
            Assert.AreEqual(SingleStageStages.OrderedStageIds[i], executeLogger.Stages[i].Stage);
        }
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddFluentWorkflow()
                .AddSingleStageWorkflow<SingleStageWorkflowImpl>(configuration =>
                {
                    configuration.AddScheduler()
                                 .AddStageSampleStage5Handler<SingleStageWorkflowSampleStage5StageHandler>();
                });

        services.AddSingleton<WorkflowExecuteLogger>();
    }

    #endregion Protected 方法
}
