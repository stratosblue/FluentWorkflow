using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// 消息消费详情
/// </summary>
[MessagePackObject]
public record MessageConsumeDetail
{
    /// <summary>
    /// 消息Id
    /// </summary>
    [Key(0)]
    public required string Id { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    [Key(1)]
    public required string EventName { get; set; }

    /// <summary>
    /// 开始处理时间
    /// </summary>
    [Key(2)]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    [Key(3)]
    public required IReadOnlyDictionary<string, string> Metadata { get; set; }
}
