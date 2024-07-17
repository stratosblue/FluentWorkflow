using FluentWorkflow.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

[TestClass]
public class RabbitMQPublisherConfirmsWorkflowSimpleExecuteTest : WorkflowSimpleExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => new RabbitMQTestServiceProviderProvider(options => options.PostConfigure<RabbitMQOptions>(options => options.PublisherConfirms = true));

    #endregion Public 方法
}
