using System.Reflection;
using FluentWorkflow.Management.Shared.Messages;

namespace FluentWorkflow.Management.Shared;

internal class SharedConstants
{
    #region Public 字段

    public static readonly string Version;

    #endregion Public 字段

    #region Public 构造函数

    static SharedConstants()
    {
        try
        {
            Version = typeof(Hello).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version!;
        }
        catch { }

        if (string.IsNullOrWhiteSpace(Version))
        {
            Version = "unknown";
        }
    }

    #endregion Public 构造函数
}
