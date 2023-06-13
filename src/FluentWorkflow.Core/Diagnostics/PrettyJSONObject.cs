using System.Text.Json;

namespace FluentWorkflow.Diagnostics;

/// <summary>
/// JSON展示对象
/// </summary>
internal static class PrettyJSONObject
{
    #region Public 方法

    /// <summary>
    /// 创建对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PrettyJSONObject<T>? Create<T>(T? value) => value is null ? null : new(value);

    #endregion Public 方法
}

/// <summary>
/// JSON展示对象
/// </summary>
/// <typeparam name="T"></typeparam>
internal class PrettyJSONObject<T>
{
    #region Private 字段

    private static readonly JsonSerializerOptions s_serializerOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 要展示的值
    /// </summary>
    public T? Value { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="PrettyJSONObject{T}"/>
    public PrettyJSONObject(T? value)
    {
        Value = value;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(Value, s_serializerOptions);

    #endregion Public 方法
}
