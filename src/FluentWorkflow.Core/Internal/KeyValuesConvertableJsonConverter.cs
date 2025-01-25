using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
/// <inheritdoc cref="IKeyValuesConvertable{T}"/> 的json转换器
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerStepThrough]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class KeyValuesConvertableJsonConverter<T>
    : JsonConverter<T>
    where T : IKeyValuesConvertable<T>
{
    #region Public 方法

    /// <inheritdoc/>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, LiteralJsonElement>>(ref reader, options);
        var keyValues = dictionary?.Select(static m => new KeyValuePair<string, string>(m.Key, m.Value.Json)) ?? [];
        return dictionary is null ? default : T.ConstructFromKeyValues(keyValues);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var keyValues = T.ConvertToKeyValues(value);
        var dictionary = keyValues.ToDictionary(static m => m.Key, m => new LiteralJsonElement(m.Value));
        JsonSerializer.Serialize(writer, dictionary, options);
    }

    #endregion Public 方法
}
