using System.ComponentModel;

namespace FluentWorkflow.Diagnostics;

/// <summary>
/// JSON展示对象
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class PrettyJSONObject
{
    #region Public 方法

    /// <summary>
    /// 创建对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PrettyJSONObject<T>? Create<T>(T? value) => Create(value, IObjectSerializer.Default);

    /// <summary>
    /// 创建对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="objectSerializer"></param>
    /// <returns></returns>
    public static PrettyJSONObject<T>? Create<T>(T? value, IObjectSerializer objectSerializer) => value is null ? null : new(value, objectSerializer);

    #endregion Public 方法
}

/// <summary>
/// JSON展示对象
/// </summary>
/// <typeparam name="T"></typeparam>
/// <inheritdoc cref="PrettyJSONObject{T}"/>
[EditorBrowsable(EditorBrowsableState.Never)]
public class PrettyJSONObject<T>(T? value, IObjectSerializer objectSerializer)
{
    #region Public 属性

    /// <summary>
    /// 要展示的值
    /// </summary>
    public T? Value { get; } = value;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        try
        {
            return objectSerializer.PrettySerialize(Value);
        }
        catch (Exception ex)
        {
            return $"Serialize \"{typeof(T)}\" failed: {ex}";
        }
    }

    #endregion Public 方法
}
