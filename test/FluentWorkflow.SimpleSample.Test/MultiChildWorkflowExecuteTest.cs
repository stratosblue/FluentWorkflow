using FluentWorkflow.SimpleSample;
using FluentWorkflow.SimpleSample.Sample;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

[TestClass]
public abstract class MultiChildWorkflowExecuteTest : FluentWorkflowTestBase
{
    #region Protected 属性

    /// <inheritdoc cref="SampleWorkflowContext.WorkWithResume"/>
    protected virtual bool WorkWithResume { get; } = false;

    #endregion Protected 属性

    #region Public 方法

    [DataRow(3, 2, 5, 3)]
    [DataRow(2, 2, 5, 3)]
    [DataRow(1, 2, 5, 3)]
    [DataRow(3, 1, 5, 3)]
    [DataRow(2, 1, 5, 3)]
    [DataRow(1, 1, 5, 3)]
    [DataRow(3, 0, 5, 3)]
    [DataRow(2, 0, 5, 3)]
    [DataRow(1, 0, 5, 3)]
    [DataRow(0, 1, 5, 3)]
    //----------------
    [DataRow(3, 2, 5, 2)]
    [DataRow(2, 2, 5, 2)]
    [DataRow(1, 2, 5, 2)]
    [DataRow(3, 1, 5, 2)]
    [DataRow(2, 1, 5, 2)]
    [DataRow(1, 1, 5, 2)]
    [DataRow(3, 0, 5, 2)]
    [DataRow(2, 0, 5, 2)]
    [DataRow(1, 0, 5, 2)]
    [DataRow(0, 1, 5, 2)]
    //----------------
    [DataRow(3, 2, 5, 1)]
    [DataRow(2, 2, 5, 1)]
    [DataRow(1, 2, 5, 1)]
    [DataRow(3, 1, 5, 1)]
    [DataRow(2, 1, 5, 1)]
    [DataRow(1, 1, 5, 1)]
    [DataRow(3, 0, 5, 1)]
    [DataRow(2, 0, 5, 1)]
    [DataRow(1, 0, 5, 1)]
    [DataRow(0, 1, 5, 1)]
    [TestMethod]
    public async Task Should_Break_With_Exception_Nest_Multi_ChildWorkflow(int depth, int stepBase, int subflowCount, int exceptionStep)
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
                MaxSubWorkflow = subflowCount,
                ExceptionStep = exceptionStep,
                ExceptionDepth = depth,
                WorkWithResume = WorkWithResume,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await Assert.ThrowsExactlyAsync<WorkflowFailureException>(() => FinishWaiterContainer[id].WaitAsync());

        //TODO 确认异常时位置正确？
        //var baseLength = SampleWorkflowStages.OrderedStageIds.Length;
        //var expected = baseLength + baseLength * (depth * subflowCount);
        //Assert.AreEqual(expected, executeLogger.Stages.Count);
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

        await Assert.ThrowsExactlyAsync<WorkflowFailureException>(() => FinishWaiterContainer[id].WaitAsync());

        //TODO 确认异常时位置正确？
        //Assert.AreEqual(SampleWorkflowStages.OrderedStageIds.Length * (depth + 1), executeLogger.Stages.Count);
    }

    [DataRow(3, 2, 5)]
    [DataRow(2, 2, 5)]
    [DataRow(1, 2, 5)]
    [DataRow(3, 1, 5)]
    [DataRow(2, 1, 5)]
    [DataRow(1, 1, 5)]
    [DataRow(3, 0, 5)]
    [DataRow(2, 0, 5)]
    [DataRow(1, 0, 5)]
    [DataRow(0, 1, 5)]
    [TestMethod]
    public async Task Should_Success_With_Nest_Multi_ChildWorkflow(int depth, int stepBase, int subflowCount)
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
                MaxSubWorkflow = subflowCount,
                WorkWithResume = WorkWithResume,
            }
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await FinishWaiterContainer[id].WaitAsync();

        var baseLength = SampleStages.OrderedStageIds.Length;
        var expected = baseLength + baseLength * (depth * subflowCount);
        Assert.HasCount(expected, executeLogger.Stages);
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

        Assert.HasCount(SampleStages.OrderedStageIds.Length * (depth + 1), executeLogger.Stages);
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddFluentWorkflow()
                .UseWorkflowAwaitProcessor<InMemoryWorkflowAwaitProcessor>()
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
