namespace FluentWorkflow;

[TestClass]
public class InMemorySingleStageWorkflowExecuteWithResumeTest : SingleStageWorkflowExecuteTest
{
    #region Protected 属性

    protected override bool WorkWithResume => true;

    #endregion Protected 属性

    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => InMemoryTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
