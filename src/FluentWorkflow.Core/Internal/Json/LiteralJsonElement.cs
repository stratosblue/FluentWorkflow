using System.Diagnostics;
using System.Text.Json.Serialization;

namespace System.Text.Json;

/// <summary>
/// 文本字面值Json对象
/// </summary>
/// <param name="Json">原始Json数据</param>
[JsonConverter(typeof(LiteralJsonElementJsonConverter))]
[DebuggerDisplay("{Json,nq}")]
[DebuggerStepThrough]
internal readonly record struct LiteralJsonElement(string Json)
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator LiteralJsonElement(string value) => new(value);

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator string(LiteralJsonElement value) => value.Json;

    /// <inheritdoc/>
    public override readonly string ToString() => Json;
}
