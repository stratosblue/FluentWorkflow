namespace FluentWorkflow;

[TestClass]
public class InMemorySingleStageWorkflowExecuteTest : SingleStageWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => InMemoryTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
