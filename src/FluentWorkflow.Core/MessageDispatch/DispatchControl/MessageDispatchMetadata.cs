using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace FluentWorkflow.MessageDispatch.DispatchControl;

/// <summary>
/// 消息调度元数据
/// </summary>
[JsonConverter(typeof(KeyValuesConvertableJsonConverter<MessageDispatchMetadata>))]
[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed class MessageDispatchMetadata(IEnumerable<KeyValuePair<string, string>>? values = null)
    : PropertyMapObject(values, StringComparer.Ordinal)
    , IKeyValuesConvertable<MessageDispatchMetadata>
{
    #region Public 属性

    /// <summary>
    /// 原始消息数据
    /// </summary>
    public ReadOnlyMemory<byte>? RawMessageData { get => InnerGet<ReadOnlyMemory<byte>>(); set => InnerSet(value); }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 设置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="name"></param>
    public void Set<T>(T? value, [CallerArgumentExpression(nameof(value))] string name = null!) => InnerSet(value, name);

    #endregion Public 方法

    #region IKeyValuesConvertable

    /// <inheritdoc/>
    public static MessageDispatchMetadata ConstructFromKeyValues(IEnumerable<KeyValuePair<string, string>> values) => new(values);

    /// <inheritdoc/>
    public static IEnumerable<KeyValuePair<string, string>> ConvertToKeyValues(MessageDispatchMetadata instance) => instance.GetSnapshot();

    #endregion IKeyValuesConvertable
}
