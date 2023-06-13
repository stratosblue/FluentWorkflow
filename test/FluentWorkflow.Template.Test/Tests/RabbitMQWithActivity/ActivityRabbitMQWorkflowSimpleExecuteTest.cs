namespace FluentWorkflow;

[TestClass]
public class ActivityRabbitMQWorkflowSimpleExecuteTest : WorkflowSimpleExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProviderWithActivity.Instance;

    #endregion Public 方法
}
