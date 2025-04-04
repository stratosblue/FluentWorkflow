﻿using System.ComponentModel;

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
[EditorBrowsable(EditorBrowsableState.Never)]
public class PrettyJSONObject<T>
{
    #region Private 字段

    private readonly IObjectSerializer _objectSerializer;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 要展示的值
    /// </summary>
    public T? Value { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="PrettyJSONObject{T}"/>
    public PrettyJSONObject(T? value, IObjectSerializer objectSerializer)
    {
        Value = value;
        _objectSerializer = objectSerializer;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        try
        {
            return _objectSerializer.PrettySerialize(Value);
        }
        catch (Exception ex)
        {
            return $"Serialize \"{typeof(T)}\" failed: {ex}";
        }
    }

    #endregion Public 方法
}
