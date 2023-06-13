namespace FluentWorkflow;

/// <summary>
/// 工作流程标识
/// </summary>
[Flags]
public enum WorkflowFlag : ulong
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0,

    /// <summary>
    /// 完成时不要进行通知
    /// </summary>
    NotNotifyOnFinish = 1 << 0,

    /// <summary>
    /// 不在意完成状态
    /// </summary>
    NotCareFinishState = 1 << 1,

    /// <summary>
    /// 具有父工作流程
    /// </summary>
    HasParentWorkflow = 1 << 2,

    /// <summary>
    /// 已被等待
    /// </summary>
    IsBeenAwaited = 1 << 3,

    /// <summary>
    /// 唯一子工作流
    /// </summary>
    UniqueChildWorkflow = 1 << 4,
}
