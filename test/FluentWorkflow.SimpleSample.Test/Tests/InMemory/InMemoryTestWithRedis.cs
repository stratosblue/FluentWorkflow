namespace FluentWorkflow;

[TestClass]
public class InMemoryRedisMultiChildWorkflowExecuteTest : RedisMultiChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => InMemoryTestServiceProviderProvider.Instance;

    #endregion Public 方法
}

[TestClass]
public class InMemoryRedisSingleChildWorkflowExecuteTest : RedisSingleChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => InMemoryTestServiceProviderProvider.Instance;

    #endregion Public 方法
}

[TestClass]
public class InMemoryRedisSingleStageWorkflowExecuteTest : RedisSingleStageWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => InMemoryTestServiceProviderProvider.Instance;

    #endregion Public 方法
}

[TestClass]
public class InMemoryRedisWorkflowSimpleExecuteTest : RedisWorkflowSimpleExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => InMemoryTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
