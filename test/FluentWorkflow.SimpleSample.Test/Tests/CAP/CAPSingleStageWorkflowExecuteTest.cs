namespace FluentWorkflow;

[TestClass]
public class CAPSingleStageWorkflowExecuteTest : SingleStageWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => CAPTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
