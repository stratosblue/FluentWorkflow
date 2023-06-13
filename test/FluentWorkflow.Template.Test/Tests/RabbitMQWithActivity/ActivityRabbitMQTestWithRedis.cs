namespace FluentWorkflow;

[TestClass]
public class ActivityRabbitMQRedisMultiChildWorkflowExecuteTest : RedisMultiChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProviderWithActivity.Instance;

    #endregion Public 方法
}

[TestClass]
public class ActivityRabbitMQRedisSingleChildWorkflowExecuteTest : RedisSingleChildWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProviderWithActivity.Instance;

    #endregion Public 方法
}

[TestClass]
public class ActivityRabbitMQRedisWorkflowSimpleExecuteTest : RedisWorkflowSimpleExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProviderWithActivity.Instance;

    #endregion Public 方法
}
