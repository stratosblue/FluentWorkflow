namespace FluentWorkflow;

/// <summary>
/// 允许使用 KeyValues 列表构建 <typeparamref name="T"/> 的对象
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IKeyValuesConvertable<T>
{
    #region Public 方法

    /// <summary>
    /// 从 KeyValues 列表构建对象
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static abstract T ConstructFromKeyValues(IEnumerable<KeyValuePair<string, string>> values);

    /// <summary>
    /// 从对象转换为 KeyValues
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static abstract IEnumerable<KeyValuePair<string, string>> ConvertToKeyValues(T instance);

    #endregion Public 方法
}
