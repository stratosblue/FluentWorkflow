namespace FluentWorkflow;

[TestClass]
public class RabbitMQRedisMultiChildWorkflowExecuteTest : RedisMultiChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProvider.Instance;

    #endregion Public 方法
}

[TestClass]
public class RabbitMQRedisSingleChildWorkflowExecuteTest : RedisSingleChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProvider.Instance;

    #endregion Public 方法
}

[TestClass]
public class RabbitMQRedisWorkflowSimpleExecuteTest : RedisWorkflowSimpleExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
