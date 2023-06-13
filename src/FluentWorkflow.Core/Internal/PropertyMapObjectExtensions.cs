using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace FluentWorkflow;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class PropertyMapObjectExtensions
{
    #region Public 方法

    #region bool

    /// <summary>
    /// 获取 bool 值
    /// </summary>
    /// <param name="propertyMapObject"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static bool? GetBoolean(this PropertyMapObject propertyMapObject, string key, bool? defaultValue = default)
        => byte.TryParse(propertyMapObject.Get(key), CultureInfo.InvariantCulture, out var value) ? value > 0 : defaultValue;

    /// <summary>
    /// 设置 bool 值
    /// </summary>
    /// <param name="propertyMapObject"></param>
    /// <param name="value"></param>
    /// <param name="key"></param>
    public static void SetBoolean(this PropertyMapObject propertyMapObject, string key, bool? value)
        => propertyMapObject.Set(key, value is null ? null : value.Value ? "1" : "0");

    #endregion bool

    #region Enum

    /// <summary>
    /// 获取 枚举 值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyMapObject"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetEnum<T>(this PropertyMapObject propertyMapObject, string key, T? defaultValue = default) where T : struct, Enum
        => Enum.TryParse<T>(propertyMapObject.Get(key), true, out var value) ? value : defaultValue;

    /// <summary>
    /// 设置 枚举 值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyMapObject"></param>
    /// <param name="value"></param>
    /// <param name="key"></param>
    public static void SetEnum<T>(this PropertyMapObject propertyMapObject, string key, T? value) where T : struct, Enum
        => propertyMapObject.Set(key, value is null ? null : ((IConvertible)value.Value).ToInt64(CultureInfo.InvariantCulture).ToString());

    #endregion Enum

    #region Nullable

    /// <summary>
    /// 获取可空类型的 <see cref="IParsable{TSelf}"/> 值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyMapObject"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetNullable<T>(this PropertyMapObject propertyMapObject, string key, T? defaultValue = default) where T : struct, IParsable<T>
        => T.TryParse(propertyMapObject.Get(key), CultureInfo.InvariantCulture, out var value) ? value : defaultValue;

    #endregion Nullable

    #region IParsable

    /// <summary>
    /// 获取 <see cref="IParsable{TSelf}"/> 值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyMapObject"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetValue<T>(this PropertyMapObject propertyMapObject, string key, T? defaultValue = default) where T : IParsable<T>
        => T.TryParse(propertyMapObject.Get(key), CultureInfo.InvariantCulture, out var value) ? value : defaultValue;

    /// <summary>
    /// 设置 <see cref="IParsable{TSelf}"/> 值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyMapObject"></param>
    /// <param name="value"></param>
    /// <param name="key"></param>
    public static void SetValue<T>(this PropertyMapObject propertyMapObject, string key, T? value) where T : IParsable<T>
        => propertyMapObject.Set(key, value?.ToString());

    #endregion IParsable

    #region Object

    /// <summary>
    /// 获取值并将其使用 <paramref name="serializer"/> 反序列化为<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyMapObject"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetObject<T>(this PropertyMapObject propertyMapObject, string key, T? defaultValue = default, IObjectSerializer? serializer = null)
        => (serializer ?? IObjectSerializer.Default).Deserialize<T>(propertyMapObject.Get(key)) ?? defaultValue;

    /// <summary>
    /// 将 <paramref name="value"/> 使用 <paramref name="serializer"/> 序列化为字符串进行设置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyMapObject"></param>
    /// <param name="value"></param>
    /// <param name="key"></param>
    /// <param name="serializer"></param>
    public static void SetObject<T>(this PropertyMapObject propertyMapObject, string key, T? value, IObjectSerializer? serializer = null)
        => propertyMapObject.Set(key, (serializer ?? IObjectSerializer.Default).Serialize(value));

    #endregion Object

    #region String

    /// <summary>
    /// 获取字符串值
    /// </summary>
    /// <param name="propertyMapObject"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static string? Get(this PropertyMapObject propertyMapObject, string key, string? defaultValue = default)
    {
        if (!propertyMapObject.DataContainer.TryGetValue(key, out var value)
            || value is null)
        {
            return defaultValue;
        }
        return value;
    }

    /// <summary>
    /// 设置字符串值
    /// </summary>
    /// <param name="propertyMapObject"></param>
    /// <param name="value"></param>
    /// <param name="key"></param>
    public static void Set(this PropertyMapObject propertyMapObject, string key, string? value)
    {
        //如果未修改值，直接返回
        if (propertyMapObject.DataContainer.TryGetValue(key, out var existedValue))
        {
            if (string.Equals(existedValue, value, StringComparison.Ordinal))
            {
                return;
            }
        }
        propertyMapObject.ThrowIfImmutable(key);
        if (value is null)
        {
            propertyMapObject.DataContainer.Remove(key);
            return;
        }
        propertyMapObject.DataContainer[key] = value;
    }

    #endregion String

    #endregion Public 方法
}
