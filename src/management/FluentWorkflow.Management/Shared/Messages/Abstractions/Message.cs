using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages.Abstractions;

/// <summary>
/// 基本消息
/// </summary>
/// <typeparam name="T"></typeparam>
[MessagePackObject]
public record Message<T> : IMessage
{
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

    /// <summary>
    /// 消息内容
    /// </summary>
    [Key(1)]
    public required T Data { get; set; }
}
