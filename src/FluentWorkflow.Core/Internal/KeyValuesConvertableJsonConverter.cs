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
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
        return dictionary is null ? default : T.ConstructFromKeyValues(dictionary);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var kvs = T.ConvertToKeyValues(value);
        var dictionary = kvs as IDictionary<string, string> ?? new Dictionary<string, string>(kvs);
        JsonSerializer.Serialize(writer, dictionary, options);
    }

    #endregion Public 方法
}
