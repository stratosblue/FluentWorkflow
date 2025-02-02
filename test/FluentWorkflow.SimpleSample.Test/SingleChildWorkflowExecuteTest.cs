using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Sample;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

[TestClass]
public abstract class SingleChildWorkflowExecuteTest : FluentWorkflowTestBase
{
    #region Protected 属性

    /// <inheritdoc cref="SampleWorkflowContext.WorkWithResume"/>
    protected virtual bool WorkWithResume { get; } = false;

    #endregion Protected 属性

    #region Public 方法

    [DataRow(9, 3)]
    [DataRow(5, 3)]
    [DataRow(2, 3)]
    //----------------
    [DataRow(9, 2)]
    [DataRow(5, 2)]
    [DataRow(2, 2)]
    //----------------
    [DataRow(9, 1)]
    [DataRow(5, 1)]
    [DataRow(2, 1)]
    [TestMethod]
    public async Task Should_Break_With_Exception_Nest_Multi_ChildWorkflow(int subflowCount, int exceptionStep)
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();

        var id = Guid.NewGuid().ToString();

        var context = new SampleWorkflowContext(id)
        {
            TestInfo = new()
            {
                StepBase = 1,
                Step = 0,
                Depth = 1,
                MaxStageDelay = 50,
                MaxSubWorkflow = subflowCount,
                ExceptionStep = exceptionStep,
                ExceptionDepth = 1,
                WorkWithResume = WorkWithResume,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await Assert.ThrowsExceptionAsync<WorkflowFailureException>(() => FinishWaiterContainer[id].WaitAsync());
    }

    [DataRow(3, 2, 3)]
    [DataRow(2, 2, 3)]
    [DataRow(1, 2, 3)]
    [DataRow(3, 1, 3)]
    [DataRow(2, 1, 3)]
    [DataRow(1, 1, 3)]
    [DataRow(3, 0, 3)]
    [DataRow(2, 0, 3)]
    [DataRow(1, 0, 3)]
    [DataRow(0, 1, 3)]
    //----------------
    [DataRow(3, 2, 1)]
    [DataRow(2, 2, 1)]
    [DataRow(1, 2, 1)]
    [DataRow(3, 1, 1)]
    [DataRow(2, 1, 1)]
    [DataRow(1, 1, 1)]
    [DataRow(3, 0, 1)]
    [DataRow(2, 0, 1)]
    [DataRow(1, 0, 1)]
    [DataRow(0, 1, 1)]
    //----------------
    [DataRow(3, 2, 2)]
    [DataRow(2, 2, 2)]
    [DataRow(1, 2, 2)]
    [DataRow(3, 1, 2)]
    [DataRow(2, 1, 2)]
    [DataRow(1, 1, 2)]
    [DataRow(3, 0, 2)]
    [DataRow(2, 0, 2)]
    [DataRow(1, 0, 2)]
    [DataRow(0, 1, 2)]
    [TestMethod]
    public async Task Should_Break_With_Exception_Nest_Single_ChildWorkflow(int depth, int stepBase, int exceptionStep)
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new SampleWorkflowContext(id)
        {
            TestInfo = new()
            {
                StepBase = stepBase,
                Step = 0,
                Depth = depth,
                ExceptionStep = exceptionStep,
                ExceptionDepth = depth,
                MaxStageDelay = 50,
                WorkWithResume = WorkWithResume,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await Assert.ThrowsExceptionAsync<WorkflowFailureException>(() => FinishWaiterContainer[id].WaitAsync());

        //TODO 确认异常时位置正确？
        //Assert.AreEqual(TemplateWorkflowStages.OrderedStageIds.Length * (depth + 1), executeLogger.Stages.Count);
    }

    [DataRow(3, 2)]
    [DataRow(2, 2)]
    [DataRow(1, 2)]
    [DataRow(3, 1)]
    [DataRow(2, 1)]
    [DataRow(1, 1)]
    [DataRow(3, 0)]
    [DataRow(2, 0)]
    [DataRow(1, 0)]
    [DataRow(0, 1)]
    [TestMethod]
    public async Task Should_Success_With_Nest_Single_ChildWorkflow(int depth, int stepBase)
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new SampleWorkflowContext(id)
        {
            TestInfo = new()
            {
                StepBase = stepBase,
                Step = 0,
                Depth = depth,
                MaxStageDelay = 50,
                WorkWithResume = WorkWithResume,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await FinishWaiterContainer[id].WaitAsync();

        Assert.AreEqual(SampleStages.OrderedStageIds.Length * (depth + 1), executeLogger.Stages.Count);
    }

    [DataRow(9)]
    [DataRow(5)]
    [DataRow(2)]
    [TestMethod]
    public virtual async Task Should_Throw_With_Nest_Multi_ChildWorkflow(int subflowCount)
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();

        var id = Guid.NewGuid().ToString();

        var context = new SampleWorkflowContext(id)
        {
            TestInfo = new()
            {
                StepBase = 1,
                Step = 0,
                Depth = 1,
                MaxStageDelay = 50,
                MaxSubWorkflow = subflowCount,
                WorkWithResume = WorkWithResume,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await Assert.ThrowsExceptionAsync<WorkflowFailureException>(() => FinishWaiterContainer[id].WaitAsync());
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
