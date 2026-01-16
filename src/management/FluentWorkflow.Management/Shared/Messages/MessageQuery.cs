using FluentWorkflow.Management.Shared.Messages.Abstractions;
using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// 消息查询请求
/// </summary>
[MessagePackObject]
public record MessageQueryRequest : PagingRequest<MessageQuery>
{
}

/// <summary>
/// 消息查询
/// </summary>
[MessagePackObject]
public record MessageQuery
{
    /// <summary>
    /// 消息Id
    /// </summary>
    [Key(0)]
    public string? MessageId { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    [Key(1)]
    public string? EventName { get; set; }
}

/// <summary>
/// 消息查询响应
/// </summary>
[MessagePackObject]
public record MessageQueryResponse : PagedResponse<MessageConsumeDetail>
{
}
