using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages.Abstractions;

/// <summary>
/// 基本响应消息
/// </summary>
/// <typeparam name="T"></typeparam>
[MessagePackObject]
public record AckMessage<T> : IAckMessage, IMessage
{
    private int? _ackId;

    private int? _id;

    /// <inheritdoc/>
    [Key(0)]
    public int Id
    {
        get => _id ?? default;
        set
        {
            if (_id.HasValue)
            {
                throw new InvalidOperationException($"{nameof(Id)} already has a value of {_id}");
            }
            _id = value;
        }
    }

    /// <inheritdoc/>
    [Key(1)]
    public int AckId
    {
        get => _ackId ?? default;
        set
        {
            if (_ackId.HasValue)
            {
                throw new InvalidOperationException($"{nameof(AckId)} already has a value of {_ackId}");
            }
            _ackId = value;
        }
    }

    /// <summary>
    /// 数据
    /// </summary>
    [Key(2)]
    public required T? Data { get; set; }

    /// <inheritdoc/>
    [Key(3)]
    public bool IsExecutionSuccess { get; set; } = true;

    /// <inheritdoc/>
    [Key(4)]
    public string? ExecutionMessage { get; set; }
}
