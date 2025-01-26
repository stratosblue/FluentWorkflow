#pragma warning disable IDE0130

namespace System.Text.Json.Serialization;

internal sealed class ActivityBaggageJsonConverter : JsonConverter<IEnumerable<KeyValuePair<string, string?>>>
{
    public override IEnumerable<KeyValuePair<string, string?>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonElement = JsonElement.ParseValue(ref reader);
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                var result = new List<KeyValuePair<string, string?>>();
                foreach (var item in jsonElement.EnumerateObject())
                {
                    result.Add(new(item.Name, item.Value.GetString()));
                }
                return result;
            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                return null;
        }
        throw new InvalidOperationException($"Can not parse \"{jsonElement.GetRawText()}\" as baggage.");
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, string?>> value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStartObject();
        foreach (var item in value)
        {
            writer.WritePropertyName(item.Key);
            writer.WriteStringValue(item.Value);
        }
        writer.WriteEndObject();
    }
}
