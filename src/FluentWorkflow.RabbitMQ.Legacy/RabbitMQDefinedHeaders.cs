namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// 已定义的 RabbitMQ Header
/// </summary>
public static class RabbitMQDefinedHeaders
{
    /// <summary>
    /// EventName在Header中的Key名称
    /// </summary>
    public const string EventName = "fwf.message.eventname";

    /// <summary>
    /// 链路追踪ID在Header中的Key名称
    /// </summary>
    public const string TraceId = "fwf.workflow.traceid";

    /// <summary>
    /// 工作流程ID在Header中的Key名称
    /// </summary>
    public const string WorkflowId = "fwf.workflow.id";
}
