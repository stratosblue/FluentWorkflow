using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FluentWorkflow;

/// <summary>
/// 基于 System.Text.Json 的 <inheritdoc cref="IObjectSerializer"/>
/// </summary>
public class SystemTextJsonObjectSerializer : IObjectSerializer
{
    #region Public 字段

    /// <summary>
    /// 序列化选项
    /// </summary>
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        AllowTrailingCommas = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
    };

    #endregion Public 字段

    #region Public 属性

    /// <summary>
    /// 静态实例
    /// </summary>
    public static SystemTextJsonObjectSerializer Instance { get; } = new();

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public T? Deserialize<T>(string? value) => string.IsNullOrWhiteSpace(value) ? default : JsonSerializer.Deserialize<T>(value, JsonSerializerOptions);

    /// <inheritdoc/>
    public T? Deserialize<T>(ReadOnlySpan<byte> value) => value.IsEmpty ? default : JsonSerializer.Deserialize<T>(value, JsonSerializerOptions);

    /// <inheritdoc/>
    public object? Deserialize(string? value, Type returnType) => string.IsNullOrWhiteSpace(value) ? default : JsonSerializer.Deserialize(value, returnType, JsonSerializerOptions);

    /// <inheritdoc/>
    public object? Deserialize(ReadOnlySpan<byte> value, Type returnType) => value.IsEmpty ? default : JsonSerializer.Deserialize(value, returnType, JsonSerializerOptions);

    /// <inheritdoc/>
    [return: NotNullIfNotNull(nameof(value))]
    public string? Serialize<T>(T? value) => value is null ? null : JsonSerializer.Serialize(value, JsonSerializerOptions);

    /// <inheritdoc/>
    [return: NotNullIfNotNull(nameof(value))]
    public byte[]? SerializeToBytes<T>(T? value) => value is null ? null : JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerOptions);

    #endregion Public 方法
}
