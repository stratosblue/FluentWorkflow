using System.Text.RegularExpressions;

namespace FluentWorkflow.Management.Shared;

/// <summary>
/// 全局选项
/// </summary>
public partial class ManagementGlobalOptions
{
    #region Private 字段

    private readonly HashSet<string> _registeredAppNames = new(StringComparer.Ordinal);

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 管理器刷新间隔
    /// </summary>
    public TimeSpan ManagerRefreshInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 已注册的应用程序名称
    /// </summary>
    public IReadOnlySet<string> RegisteredAppNames => _registeredAppNames;

    /// <summary>
    /// 已注册的工作器名称
    /// </summary>
    public HashSet<string> RegisteredWorkerNames { get; set; } = new(StringComparer.Ordinal);

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 注册一个应用程序名称
    /// </summary>
    /// <param name="appName"></param>
    public void RegisterAppName(string appName)
    {
        if (GetAppNameRegex().IsMatch(appName))
        {
            _registeredAppNames.Add(appName);
        }
        else
        {
            throw new ArgumentException($"App name \"{appName}\" invalid. The name must be a combination of two or more lowercase letters and an underline.");
        }
    }

    #endregion Public 方法

    #region Private 方法

    [GeneratedRegex(@"[a-z_]{2,}")]
    private partial Regex GetAppNameRegex();

    #endregion Private 方法
}
