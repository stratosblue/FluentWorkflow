//using FluentWorkflow.Interface;
//using FluentWorkflow.SharedSample;
//using FluentWorkflow.SharedSample.SharedXX;
//using Microsoft.Extensions.DependencyInjection;

//namespace FluentWorkflow;

//[TestClass]
//public class SharedXXWorkflowExecuteTest : FluentWorkflowTestBase
//{
//    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProvider.Instance;

//    #region Public 方法

//    [TestMethod]
//    public async Task Should_Run_Success()
//    {
//        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SharedXXWorkflow>>();

//        var executeLogger = ServiceProvider.GetRequiredService<WorkflowExecuteLogger>();

//        var id = Guid.NewGuid().ToString();

//        var context = new FluentWorkflow.SharedSample.SharedXXWorkflowContext(id)
//        {
//            TestInfo = new()
//        };
//        var workflow = workflowBuilder.Build(context);

//        await workflow.StartAsync(default);

//        await FinishWaiterContainer[id].WaitAsync();

//        Assert.AreEqual(SharedXXStages.OrderedStageIds.Length, executeLogger.Stages.Count);

//        for (int i = 0; i < executeLogger.Stages.Count; i++)
//        {
//            Assert.AreEqual(SharedXXStages.OrderedStageIds[i], executeLogger.Stages[i].Stage);
//        }
//    }

//    #endregion Public 方法

//    #region Protected 方法

//    protected override void ConfigureServices(IServiceCollection services)
//    {
//        services.AddFluentWorkflow()
//                .AddSharedXXWorkflow(configuration =>
//                {
//                    configuration.AddScheduler()
//                                 .AddResultObserver()
//                                 .AddStageSampleStage1Handler<SampleWorkflowSampleStage1StageHandler>()
//                                 .AddStageSampleStage2Handler<SampleWorkflowSampleStage2StageHandler>()
//                                 .AddStageSampleStage3Handler<SampleWorkflowSampleStage3StageHandler>();
//                });


//        services.AddSingleton<WorkflowExecuteLogger>();
//    }

//    #endregion Protected 方法
//}
