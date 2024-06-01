using System.Reflection;

namespace FluentWorkflow;

/// <summary>
/// 工作流程环境信息
/// </summary>
public static class FluentWorkflowEnvironment
{
    #region Public 属性

    /// <summary>
    /// 获取描述
    /// </summary>
    public static string Description => $"{HostAssemblyName}@{HostName}[{Environment.ProcessId}]";

    /// <summary>
    /// 宿主程序集名称，优先使用环境变量 "FLUENT_WORKFLOW_HOSTASSEMBLYNAME"，默认情况下通常为 <see cref="Assembly.GetEntryAssembly"/> -> <see cref="Assembly.GetName()"/> -> <see cref="AssemblyName.Name"/>
    /// </summary>
    public static string HostAssemblyName { get; }

    /// <summary>
    /// 宿主名称，优先使用环境变量 "FLUENT_WORKFLOW_HOSTNAME"，默认情况下通常为 <see cref="Environment.MachineName"/>
    /// </summary>
    public static string HostName { get; }

    #endregion Public 属性

    #region Public 构造函数

    static FluentWorkflowEnvironment()
    {
        var hostName = Environment.GetEnvironmentVariable("FLUENT_WORKFLOW_HOSTNAME");

        if (string.IsNullOrEmpty(hostName))
        {
            try
            {
                hostName = Environment.MachineName;
            }
            catch
            {
                hostName = RandomName();
            }
        }

        HostName = hostName;

        var hostAssemblyName = Environment.GetEnvironmentVariable("FLUENT_WORKFLOW_HOSTASSEMBLYNAME");

        if (string.IsNullOrEmpty(hostAssemblyName))
        {
            try
            {
                hostAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? RandomName();
            }
            catch
            {
                hostAssemblyName = RandomName();
            }
        }

        HostAssemblyName = hostAssemblyName;

        static string RandomName()
        {
            var guid = Guid.NewGuid().ToString();
            return guid.Substring(0, guid.IndexOf('-'));
        }
    }

    #endregion Public 构造函数
}
