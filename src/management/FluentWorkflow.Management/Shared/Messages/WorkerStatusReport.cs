using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// Worker状态上报
/// </summary>
[MessagePackObject]
public record WorkerStatusReport
{
    /// <summary>
    /// 处理中数量
    /// </summary>
    [Key(0)]
    public int ProcessingCount { get; set; }
}
