using FluentWorkflow.GenericExtension.FluentWorkflowSimpleSample;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FluentWorkflow;

[TestClass]
public abstract class RedisMultiChildWorkflowExecuteTest : MultiChildWorkflowExecuteTest
{
    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddFluentWorkflow().UseRedisWorkflowAwaitProcessor<TestSimpleRedisConnectionProvider>();
    }

    #endregion Protected 方法
}

[TestClass]
public abstract class RedisSingleChildWorkflowExecuteTest : SingleChildWorkflowExecuteTest
{
    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddFluentWorkflow().UseRedisWorkflowAwaitProcessor<TestSimpleRedisConnectionProvider>();
    }

    #endregion Protected 方法

    #region Public 方法

    public override Task Should_Throw_With_Nest_Multi_ChildWorkflow(int subflowCount)
    {
        return base.Should_Throw_With_Nest_Multi_ChildWorkflow(subflowCount);
    }

    #endregion Public 方法
}

[TestClass]
public abstract class RedisSingleStageWorkflowExecuteTest : SingleStageWorkflowExecuteTest
{
    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddFluentWorkflow().UseRedisWorkflowAwaitProcessor<TestSimpleRedisConnectionProvider>();
    }

    #endregion Protected 方法
}

[TestClass]
public abstract class RedisWorkflowSimpleExecuteTest : WorkflowSimpleExecuteTest
{
    #region Protected 方法

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddFluentWorkflow().UseRedisWorkflowAwaitProcessor<TestSimpleRedisConnectionProvider>();
    }

    #endregion Protected 方法
}

#region Internal

internal class TestSimpleRedisConnectionProvider : IFluentWorkflowRedisConnectionProvider
{
    #region Private 字段

    private readonly ConnectionMultiplexer _connectionMultiplexer;

    #endregion Private 字段

    #region Public 构造函数

    public TestSimpleRedisConnectionProvider(IConfiguration configuration)
    {
        _connectionMultiplexer = ConnectionMultiplexer.Connect(configuration.GetValue<string>("Redis")!);
    }

    #endregion Public 构造函数

    #region Public 方法

    public IConnectionMultiplexer Get() => _connectionMultiplexer;

    #endregion Public 方法
}

#endregion Internal
