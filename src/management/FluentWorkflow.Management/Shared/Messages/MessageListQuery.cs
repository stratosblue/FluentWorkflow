using FluentWorkflow.Management.Shared.Messages.Abstractions;
using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// 消息列表查询请求
/// </summary>
[MessagePackObject]
public record MessageListQueryRequest : PagingRequest<MessageListQuery>
{
}

/// <summary>
/// 消息列表查询
/// </summary>
[MessagePackObject]
public record MessageListQuery
{
    /// <summary>
    /// 事件名称
    /// </summary>
    [Key(0)]
    public string? EventName { get; set; }
}

/// <summary>
/// 消息列表查询响应
/// </summary>
[MessagePackObject]
public record MessageListQueryResponse : PagedResponse<MessageBasicInfo>
{
}

/// <summary>
/// 消息基本信息
/// </summary>
[MessagePackObject]
public record MessageBasicInfo
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
}
