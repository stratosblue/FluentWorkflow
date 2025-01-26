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
    #region Private 静态字段

    private static readonly IReadOnlyDictionary<string, string> s_empty = ImmutableDictionary.Create<string, string>();

    #endregion Private 静态字段

    #region Private 字段

    private IObjectSerializer _objectSerializer = IObjectSerializer.Default;

    #endregion Private 字段

    #region Protected 属性

    /// <summary>
    /// 原始数据容器
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal Dictionary<string, string> DataContainer { get; }

    /// <summary>
    /// 引用对象容器
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal Dictionary<string, object> ObjectContainer { get; }

    /// <summary>
    /// 获取对象时使用的对象序列化器
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected internal IObjectSerializer ObjectSerializer
    {
        get => _objectSerializer;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _objectSerializer = value;
        }
    }

    #endregion Protected 属性

    #region Public 构造函数

    /// <inheritdoc cref="PropertyMapObject"/>
    [DebuggerStepThrough]
    public PropertyMapObject(StringComparer comparer)
    {
        DataContainer = new(8, comparer);
        ObjectContainer = new(8, comparer);
    }

    /// <inheritdoc cref="PropertyMapObject"/>
    [DebuggerStepThrough]
    public PropertyMapObject(IEnumerable<KeyValuePair<string, string>>? collection, StringComparer comparer)
    {
        DataContainer = new(collection ?? s_empty, comparer);
        ObjectContainer = new(DataContainer.Count, comparer);
    }

    #endregion Public 构造函数

    #region Protected Internal 方法

    /// <summary>
    /// 获取当前数据的快照 (引用对象的变更将会固化在返回数据中)
    /// </summary>
    /// <returns></returns>
    protected internal IReadOnlyDictionary<string, string> GetSnapshot()
    {
        var snapshot = new Dictionary<string, string>((DataContainer.Count + ObjectContainer.Count) / 2, DataContainer.Comparer);
        //先存放对象数据
        foreach (var (key, value) in ObjectContainer)
        {
            snapshot.Add(key, ObjectSerializer.Serialize(value));
        }
        //补充未序列化的原始数据
        foreach (var (key, value) in DataContainer)
        {
            snapshot.TryAdd(key, value);
        }
        return snapshot;
    }

    #endregion Protected Internal 方法

    #region Inner

    /// <summary>
    /// 获取值 <paramref name="propName"/> 的类型化值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="defaultValue"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected internal T? InnerGet<T>(T? defaultValue = default, [CallerMemberName] string propName = null!)
    {
        if (ObjectContainer.TryGetValue(propName, out var objectValue))
        {
            return (T)objectValue;
        }

        if (DataContainer.TryGetValue(propName, out var value)
            && value is not null)
        {
            objectValue = ObjectSerializer.Deserialize<T>(value) ?? defaultValue;
        }
        else
        {
            objectValue = defaultValue;
        }

        //缓存对象
        InnerSetWithoutValidation(objectValue, propName);

        return (T?)objectValue;
    }

    /// <summary>
    /// 设置 <paramref name="propName"/> 的值为 <paramref name="value"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="propName"></param>
    protected internal void InnerSet<T>(T? value, [CallerMemberName] string propName = null!)
    {
        ThrowIfImmutable(propName);

        InnerSetWithoutValidation(value, propName);
    }

    /// <summary>
    /// 不进行验证，直接设置 <paramref name="propName"/> 的值为 <paramref name="value"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="propName"></param>
    protected internal void InnerSetWithoutValidation<T>(T? value, [CallerMemberName] string propName = null!)
    {
        if (value is null)
        {
            DataContainer.Remove(propName);
            ObjectContainer.Remove(propName);
            return;
        }
        ObjectContainer[propName] = value;
    }

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
