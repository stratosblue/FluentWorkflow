namespace FluentWorkflow;

[TestClass]
public class RabbitMQSingleChildWorkflowExecuteTest : SingleChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
