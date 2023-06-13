namespace FluentWorkflow;

[TestClass]
public class ActivityRabbitMQSingleStageWorkflowExecuteTest : SingleStageWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProviderWithActivity.Instance;

    #endregion Public 方法
}
