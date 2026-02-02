using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Management;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.Management.Shared.Messages.Abstractions;
using Hoarwell;
using Hoarwell.Enhancement;

namespace FluentWorkflow.Management.Shared.Communication;

internal sealed partial class HoarwellTypeIdentifierAnalyzer : ITypeIdentifierAnalyzer
{
    #region Private 字段

    //增加消息只能在末尾追加，移除消息需要保留占位
    private static readonly FrozenDictionary<ushort, Type> s_allMessageTypes = new MessageTypeMapBuilder<ushort>(1000, static m => (ushort)(m + 1))
        .Reserve()
        .Message<Hello>()
        .AckMessage<Welcome>()
        .Message<Ping>()
        .AckMessage<Pong>()
        .Message<Close>()
        .Message<MessageListQueryRequest>()
        .AckMessage<MessageListQueryResponse>()
        .Message<MessageQueryRequest>()
        .AckMessage<MessageQueryResponse>()
        .Message<ConsumptionControl>()
        .AckMessage<ConsumptionControlResult>()
        .Message<WorkerStatusReport>()
        .Message<WorkerStatisticsRequest>()
        .AckMessage<WorkerStatistics>()
        .Build();

    private readonly FrozenDictionary<ReadOnlyMemory<byte>, Type> _identifierToTypeMap;

    private readonly FrozenDictionary<Type, ReadOnlyMemory<byte>> _typeToIdentifierMap;

    public HoarwellTypeIdentifierAnalyzer()
    {
        var identifierToTypeMap = s_allMessageTypes.ToFrozenDictionary(keySelector: static m => BitConverter.GetBytes(m.Key),
                                                                       elementSelector: static m => m.Value,
                                                                       comparer: ReadOnlyMemoryByteEqualityComparer.Shared);
        _identifierToTypeMap = identifierToTypeMap;
        _typeToIdentifierMap = identifierToTypeMap.ToFrozenDictionary(static m => m.Value, static m => m.Key);
    }

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public uint TypeIdentifierSize { get; } = sizeof(ushort);

    #endregion Public 属性

    #region Public 方法

    public bool TryGetIdentifier(in Type type, Span<byte> destination)
    {
        if (_typeToIdentifierMap.TryGetValue(type, out var value))
        {
            value.Span.CopyTo(destination);
            return true;
        }
        return false;
    }

    public bool TryGetIdentifier(in ReadOnlySequence<byte> input, Span<byte> destination)
    {
        if (input.Length >= TypeIdentifierSize)
        {
            input.Slice(0, TypeIdentifierSize).CopyTo(destination);
            return true;
        }
        return false;
    }

    public bool TryGetIdentifier(in Type type, out ReadOnlyMemory<byte> identifier)
    {
        return _typeToIdentifierMap.TryGetValue(type, out identifier);
    }

    public bool TryGetIdentifier(in ReadOnlySequence<byte> input, out ReadOnlyMemory<byte> identifier)
    {
        if (input.Length >= TypeIdentifierSize)
        {
            // Additional performance overhead about ToArray()
            identifier = input.Slice(0, TypeIdentifierSize).ToArray();
            return true;
        }
        identifier = default;
        return false;
    }

    public bool TryGetType(in ReadOnlySpan<byte> identifier, [NotNullWhen(true)] out Type? type)
    {
        // Additional performance overhead about ToArray()
        return _identifierToTypeMap.TryGetValue(identifier.ToArray(), out type);
    }

    #endregion Public 方法

    #region Internal 类

    internal class MessageTypeMapBuilder<TIdentifier>(TIdentifier initIdentifier, Func<TIdentifier, TIdentifier> nextIdentifier)
        where TIdentifier : notnull
    {
        #region Private 字段

        private readonly Dictionary<TIdentifier, Type> _messageTypes = [];

        private TIdentifier _identifier = initIdentifier;

        #endregion Private 字段

        #region Public 方法

        public MessageTypeMapBuilder<TIdentifier> AckMessage<T>()
        {
            _messageTypes.Add(_identifier, typeof(AckMessage<T>));
            _identifier = nextIdentifier(_identifier);
            return this;
        }

        public FrozenDictionary<TIdentifier, Type> Build() => _messageTypes.ToFrozenDictionary();

        public MessageTypeMapBuilder<TIdentifier> Message<T>()
        {
            _messageTypes.Add(_identifier, typeof(Message<T>));
            _identifier = nextIdentifier(_identifier);
            return this;
        }

        /// <summary>
        /// 保留位
        /// </summary>
        /// <returns></returns>
        public MessageTypeMapBuilder<TIdentifier> Reserve(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                _identifier = nextIdentifier(_identifier);
            }
            return this;
        }

        #endregion Public 方法
    }

    #endregion Internal 类
}
