﻿namespace FluentWorkflow;

[TestClass]
public class RabbitMQSingleStageWorkflowExecuteTest : SingleStageWorkflowExecuteTest
{
    #region Public 方法

    public override TestServiceProviderProvider GetProvider() => RabbitMQTestServiceProviderProvider.Instance;

    #endregion Public 方法
}
