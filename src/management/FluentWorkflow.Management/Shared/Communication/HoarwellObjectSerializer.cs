using System.Buffers;
using MessagePack;

namespace FluentWorkflow.Management.Shared.Communication;

internal sealed class HoarwellObjectSerializer : Hoarwell.IObjectSerializer
{
    #region Public 方法

    public object? Deserialize(Type type, ReadOnlySequence<byte> data)
    {
        return MessagePackSerializer.Deserialize(type, data);
    }

    public void Serialize(Type type, object? value, IBufferWriter<byte> bufferWriter)
    {
        MessagePackSerializer.Serialize(type, bufferWriter, value);
    }

    #endregion Public 方法
}
