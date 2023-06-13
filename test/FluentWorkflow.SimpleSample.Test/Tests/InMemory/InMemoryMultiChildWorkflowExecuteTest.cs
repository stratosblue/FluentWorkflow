namespace FluentWorkflow;

[TestClass]
public class InMemoryMultiChildWorkflowExecuteTest : MultiChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => InMemoryTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
