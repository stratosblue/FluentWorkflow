﻿using FluentWorkflow.Interface;
using FluentWorkflow.SimpleSample;
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
            ExceptionStep = exceptionStep,
        };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await Assert.ThrowsExceptionAsync<WorkflowFailureException>(() => FinishWaiterContainer[id].WaitAsync());

        Assert.AreEqual(exceptionStep, executeLogger.Stages.Count);
    }

    [TestMethod]
    public async Task Should_Run_Success()
    {
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new SampleWorkflowContext(id);
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await FinishWaiterContainer[id].WaitAsync();

        Assert.AreEqual(SampleWorkflowStages.OrderedStageIds.Length, executeLogger.Stages.Count);

        for (int i = 0; i < executeLogger.Stages.Count; i++)
        {
            Assert.AreEqual(SampleWorkflowStages.OrderedStageIds[i], executeLogger.Stages[i].Stage);
        }
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddFluentWorkflow()
                .AddSampleWorkflowScheduler<SampleWorkflowImpl>()
                .AddSampleWorkflowResultObserver()
                .AddSampleWorkflowSampleStage1StageHandler<SampleWorkflowSampleStage1StageHandler>()
                .AddSampleWorkflowSampleStage2StageHandler<SampleWorkflowSampleStage2StageHandler>()
                .AddSampleWorkflowSampleStage3StageHandler<SampleWorkflowSampleStage3StageHandler>();

        services.AddSingleton<WorkflowExecuteLogger>();
    }

    #endregion Protected 方法
}
