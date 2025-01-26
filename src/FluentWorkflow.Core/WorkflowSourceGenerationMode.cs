namespace FluentWorkflow;

/// <summary>
/// 工作流程生成模式
/// </summary>
[Flags]
public enum WorkflowSourceGenerationMode : uint
{
    /// <summary>
    /// 默认
    /// </summary>
    Default = 0,

    /// <summary>
    /// 工作流程
    /// </summary>
    Workflow = 1 << 0,

    /// <summary>
    /// 消息
    /// </summary>
    Messages = 1 << 1,

    /// <summary>
    /// 消息处理器
    /// </summary>
    MessageHandlers = 1 << 2,

    /// <summary>
    /// 调度器
    /// </summary>
    Scheduler = 1 << 3,

    /// <summary>
    /// 全部
    /// </summary>
    All = 0xFFFFFFFF,
}
