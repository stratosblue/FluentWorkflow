using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FluentWorkflow;

/// <summary>
/// 属性映射对象<para/>
/// 属性将作为字符串存放在字典中<para/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class PropertyMapObject
{
    //TODO 测试

    #region Private 静态字段

    private static readonly IReadOnlyDictionary<string, string> s_empty = ImmutableDictionary.Create<string, string>();

    #endregion Private 静态字段

    #region Protected 索引器

    /// <summary>
    /// <see cref="DataContainer"/> 索引器（无验证）
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected internal string this[string key] { get => DataContainer[key]; set => DataContainer[key] = value; }

    #endregion Protected 索引器

    #region Protected 字段

    /// <summary>
    /// 数据容器
    /// </summary>
    protected internal readonly Dictionary<string, string> DataContainer;

    #endregion Protected 字段

    #region Public 构造函数

    /// <inheritdoc cref="PropertyMapObject"/>
    [DebuggerStepThrough]
    public PropertyMapObject(StringComparer comparer)
    {
        DataContainer = new(8, comparer);
    }

    /// <inheritdoc cref="PropertyMapObject"/>
    [DebuggerStepThrough]
    public PropertyMapObject(IEnumerable<KeyValuePair<string, string>>? collection, StringComparer comparer)
    {
        DataContainer = new(collection ?? s_empty, comparer);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 转换为字符串，类似 K1=V1; K2=V2....
    /// </summary>
    /// <returns></returns>
    public virtual string ToFormatString() => string.Join("; ", DataContainer.Select(m => $"{m.Key}={m.Value}"));

    #endregion Public 方法

    #region Inner

    #region bool

    /// <summary>
    /// 获取枚举值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected bool? InnerGetBoolean(bool? defaultValue = default, [CallerMemberName] string propName = null!) => this.GetBoolean(propName, defaultValue);

    /// <summary>
    /// 设置枚举值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="propName"></param>
    protected void InnerSetBoolean(bool? value, [CallerMemberName] string propName = null!) => this.SetBoolean(propName, value);

    #endregion bool

    #region Enum

    /// <summary>
    /// 获取枚举值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected T? InnerGetEnum<T>(T? defaultValue = default, [CallerMemberName] string propName = null!) where T : struct, Enum => this.GetEnum<T>(propName, defaultValue);

    /// <summary>
    /// 设置枚举值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="propName"></param>
    protected void InnerSetEnum<T>(T? value, [CallerMemberName] string propName = null!) where T : struct, Enum => this.SetEnum(propName, value);

    #endregion Enum

    #region Nullable

    /// <summary>
    /// 获取可空类型的 <see cref="IParsable{TSelf}"/> 值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="propName"></param>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected void InnerGetNullable<T>(T? defaultValue = default, [CallerMemberName] string propName = null!) where T : struct, IParsable<T> => this.GetNullable(propName, defaultValue);

    #endregion Nullable

    #region IParsable

    /// <summary>
    /// 获取 <see cref="IParsable{TSelf}"/> 值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="defaultValue"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected T? InnerGetValue<T>(T? defaultValue = default, [CallerMemberName] string propName = null!) where T : IParsable<T> => this.GetValue<T>(propName, defaultValue);

    /// <summary>
    /// 设置 <see cref="IParsable{TSelf}"/> 值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="propName"></param>
    protected void InnerSetValue<T>(T? value, [CallerMemberName] string propName = null!) where T : IParsable<T> => this.SetValue<T>(propName, value);

    #endregion IParsable

    #region Object

    /// <summary>
    /// 获取值并将其使用 <paramref name="serializer"/> 反序列化为<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="defaultValue"></param>
    /// <param name="serializer"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected T? InnerGetObject<T>(T? defaultValue = default, IObjectSerializer? serializer = null, [CallerMemberName] string propName = null!) => this.GetObject<T>(propName, defaultValue, serializer);

    /// <summary>
    /// 将 <paramref name="value"/> 使用 <paramref name="serializer"/> 序列化为字符串进行设置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    /// <param name="propName"></param>
    protected void InnerSetObject<T>(T? value, IObjectSerializer? serializer = null, [CallerMemberName] string propName = null!) => this.SetObject<T>(propName, value, serializer);

    #endregion Object

    #region String

    /// <summary>
    /// 获取字符串值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected string? InnerGet(string? defaultValue = default, [CallerMemberName] string propName = null!) => this.Get(propName, defaultValue);

    /// <summary>
    /// 设置字符串值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="propName"></param>
    protected void InnerSet(string? value, [CallerMemberName] string propName = null!) => this.Set(propName, value);

    #endregion String

    #endregion Inner

    #region Internal 方法

    internal void ThrowIfImmutable(string key)
    {
        if (IsMutable(key))
        {
            return;
        }
        throw new InvalidOperationException($"\"{key}\" is immutable.");
    }

    /// <summary>
    /// 检查<paramref name="key"/>是否可变
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected internal virtual bool IsMutable(string key) => true;

    #endregion Internal 方法
}
