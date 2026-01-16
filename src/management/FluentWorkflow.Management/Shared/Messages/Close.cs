using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// 关闭消息
/// </summary>
[MessagePackObject]
public record Close
{
    /// <summary>
    /// 关闭原因
    /// </summary>
    [Key(0)]
    public required string Reason { get; set; }
}
