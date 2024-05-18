using System.Diagnostics.CodeAnalysis;

namespace FluentWorkflow;

/// <summary>
/// 对象序列化器
/// </summary>
public interface IObjectSerializer
{
    #region Public 属性

    /// <summary>
    /// 默认实例
    /// </summary>
    public static IObjectSerializer Default { get; } = SystemTextJsonObjectSerializer.Shared;

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 反序列化字符串为对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public T? Deserialize<T>(string? value);

    /// <summary>
    /// 反序列化字符串为对象
    /// </summary>
    /// <param name="value"></param>
    /// <param name="returnType"></param>
    /// <returns></returns>
    public object? Deserialize(string? value, Type returnType);

    /// <summary>
    /// 反序列化bytes为对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public T? Deserialize<T>(ReadOnlySpan<byte> value);

    /// <summary>
    /// 反序列化bytes为对象
    /// </summary>
    /// <param name="value"></param>
    /// <param name="returnType"></param>
    /// <returns></returns>
    public object? Deserialize(ReadOnlySpan<byte> value, Type returnType);

    /// <summary>
    /// 序列化对象为利于阅读的字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public string PrettySerialize<T>(T? value);

    /// <summary>
    /// 序列化对象为字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public string Serialize<T>(T? value);

    /// <summary>
    /// 序列化对象为bytes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(value))]
    public byte[]? SerializeToBytes<T>(T? value);

    #endregion Public 方法
}
