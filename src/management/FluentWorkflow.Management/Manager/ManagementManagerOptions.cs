namespace FluentWorkflow.Management.Manager;

/// <summary>
/// 管理器选项
/// </summary>
public class ManagementManagerOptions
{
    #region Public 属性

    /// <summary>
    /// Cookie
    /// </summary>
    public required string Cookie { get; set; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 有效性验证
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public ManagementManagerOptions Validation()
    {
        if (string.IsNullOrWhiteSpace(Cookie) || Cookie.Length < 16)
        {
            throw new ArgumentException($"The length of \"{nameof(Cookie)}\" must be greater than 16");
        }
        return this;
    }

    #endregion Public 方法
}
