namespace FluentWorkflow;

[TestClass]
public class CAPRedisMultiChildWorkflowExecuteTest : RedisMultiChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => CAPTestServiceProviderProvider.Instance;

    #endregion Public 方法
}

[TestClass]
public class CAPRedisSingleChildWorkflowExecuteTest : RedisSingleChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => CAPTestServiceProviderProvider.Instance;

    #endregion Public 方法
}

[TestClass]
public class CAPRedisWorkflowSimpleExecuteTest : RedisWorkflowSimpleExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => CAPTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
