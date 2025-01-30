namespace FluentWorkflow;

/// <summary>
/// 工作流程失败信息
/// </summary>
/// <param name="Stage">失败阶段</param>
/// <param name="Message">失败消息</param>
/// <param name="StackTrace">失败堆栈追踪</param>
public record class WorkflowFailureInformation(string Stage, string Message, string? StackTrace);
