using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// Worker统计请求
/// </summary>
[MessagePackObject]
public record WorkerStatisticsRequest
{
}

/// <summary>
/// Worker统计
/// </summary>
[MessagePackObject]
public record WorkerStatistics
{
    /// <summary>
    /// 处理中数量
    /// </summary>
    [Key(0)]
    public int ProcessingCount { get; set; }

    /// <summary>
    /// 消息的时序统计列表
    /// </summary>
    [Key(1)]
    public required IList<WorkerMessageTimeSequenceStatistics> MessageStatistics { get; set; }
}

/// <summary>
/// Worker消息的时序统计
/// </summary>
[MessagePackObject]
public record WorkerMessageTimeSequenceStatistics
{
    /// <summary>
    /// 时间跨度
    /// </summary>
    [Key(0)]
    public TimeSpan TimeSpan { get; set; }

    /// <summary>
    /// 传入的消息数量
    /// </summary>
    [Key(1)]
    public int IncomingCount { get; set; }

    /// <summary>
    /// 完成的消息数量
    /// </summary>
    [Key(2)]
    public int CompletedCount { get; set; }

    /// <summary>
    /// 平均处理时长
    /// </summary>
    [Key(3)]
    public TimeSpan AverageProcessingTimeSpan { get; set; }
}
