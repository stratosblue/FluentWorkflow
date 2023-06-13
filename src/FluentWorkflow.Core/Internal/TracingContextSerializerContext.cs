using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentWorkflow.Tracing;

namespace FluentWorkflow.Internal;

[JsonSerializable(typeof(TracingContext), GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal partial class TracingContextSerializerContext : JsonSerializerContext
{
    #region Public 属性

    public static TracingContextSerializerContext CustomInstance { get; } = new(new JsonSerializerOptions()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IncludeFields = false,
        IgnoreReadOnlyProperties = false,
    });

    #endregion Public 属性
}
