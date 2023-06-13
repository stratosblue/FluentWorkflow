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
    public static IObjectSerializer Default { get; } = SystemTextJsonObjectSerializer.Instance;

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
    /// 反序列化bytes为对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public T? Deserialize<T>(ReadOnlySpan<byte> value);

    /// <summary>
    /// 序列化对象为字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(value))]
    public string? Serialize<T>(T? value);

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
