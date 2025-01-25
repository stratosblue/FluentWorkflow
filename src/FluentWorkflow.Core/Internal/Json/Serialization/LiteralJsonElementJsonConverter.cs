namespace System.Text.Json.Serialization;

/// <summary>
/// <see cref="LiteralJsonElement"/> 的Json转换器
/// </summary>
internal sealed class LiteralJsonElementJsonConverter : JsonConverter<LiteralJsonElement>
{
    /// <inheritdoc/>
    public override LiteralJsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonElement.ParseValue(ref reader).GetRawText();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, LiteralJsonElement value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Json, true);
    }
}
