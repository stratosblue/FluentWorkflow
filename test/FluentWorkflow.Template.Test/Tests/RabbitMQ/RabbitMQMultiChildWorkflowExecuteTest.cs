namespace FluentWorkflow;

[TestClass]
public class RabbitMQMultiChildWorkflowExecuteTest : MultiChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
