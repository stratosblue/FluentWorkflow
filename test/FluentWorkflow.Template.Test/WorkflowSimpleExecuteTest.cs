using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;
using TemplateNamespace;
using TemplateNamespace.Template;

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
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<TemplateWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new TemplateWorkflowContext(id)
        {
            TestInfo = new()
            {
                ExceptionStep = exceptionStep,
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
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<TemplateWorkflow>>();

        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

        var id = Guid.NewGuid().ToString();

        var context = new TemplateWorkflowContext(id) { TestInfo = new() };
        var workflow = workflowBuilder.Build(context);

        await workflow.StartAsync(default);

        await FinishWaiterContainer[id].WaitAsync();

        Assert.AreEqual(TemplateStages.OrderedStageIds.Length, executeLogger.Stages.Count);

        for (int i = 0; i < executeLogger.Stages.Count; i++)
        {
            Assert.AreEqual(TemplateStages.OrderedStageIds[i], executeLogger.Stages[i].Stage);
        }
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddFluentWorkflow()
                .AddTemplateWorkflow<TemplateWorkflowImpl>(configuration =>
                {
                    configuration.AddScheduler()
                                 .AddResultObserver()
                                 .AddStageStage1CAUKHandler<TemplateWorkflowStage1CAUKStageHandler>()
                                 .AddStageStage2BPTGHandler<TemplateWorkflowStage2BPTGStageHandler>()
                                 .AddStageStage3AWBNHandler<TemplateWorkflowStage3AWBNStageHandler>();
                });

        services.AddSingleton<WorkflowExecuteLogger>();
    }

    #endregion Protected 方法
}
