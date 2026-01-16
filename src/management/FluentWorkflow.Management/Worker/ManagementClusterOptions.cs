namespace FluentWorkflow.Management.Worker;

/// <summary>
/// 管理集群选项
/// </summary>
public class ManagementClusterOptions
{
    #region Public 属性

    /// <summary>
    /// Cookie
    /// </summary>
    public required string Cookie { get; set; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 验证有效性
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public ManagementClusterOptions Validation()
    {
        if (string.IsNullOrWhiteSpace(Cookie) || Cookie.Length < 16)
        {
            throw new ArgumentException($"The length of \"{nameof(Cookie)}\" must be greater than 16");
        }
        return this;
    }

    #endregion Public 方法
}
