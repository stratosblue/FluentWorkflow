using FluentWorkflow.MessageDispatch.DispatchControl;
using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// 消费控制
/// </summary>
[MessagePackObject]
public record ConsumptionControl
{
    /// <summary>
    /// 目标消息Id
    /// </summary>
    [Key(0)]
    public required string TargetMessageId { get; set; }

    /// <summary>
    /// 控制类型
    /// </summary>
    [Key(1)]
    public required ConsumptionControlType Type { get; set; }

    /// <summary>
    /// 原因
    /// </summary>
    [Key(2)]
    public required string Reason { get; set; }
}

/// <summary>
/// 消费控制结果
/// </summary>
[MessagePackObject]
public record ConsumptionControlResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [Key(0)]
    public required bool IsSuccess { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [Key(1)]
    public string? Message { get; set; }
}
