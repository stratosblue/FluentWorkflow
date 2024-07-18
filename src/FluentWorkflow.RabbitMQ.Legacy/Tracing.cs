using System.Diagnostics;
using FluentWorkflow.Diagnostics;

namespace FluentWorkflow.RabbitMQ;

internal class Tracing
{
    #region Public 字段

    public static readonly ActivitySource ConsumerActivitySource = new($"{DiagnosticConstants.ActivityNames.RootActivitySourceName}.RabbitMQConsumer");

    public static readonly ActivitySource PublisherActivitySource = new($"{DiagnosticConstants.ActivityNames.RootActivitySourceName}.RabbitMQPublisher");

    #endregion Public 字段
}
