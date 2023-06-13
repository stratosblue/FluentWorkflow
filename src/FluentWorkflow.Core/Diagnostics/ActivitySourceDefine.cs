using System.Diagnostics;

namespace FluentWorkflow.Diagnostics;

internal static class ActivitySourceDefine
{
    #region Public 属性

    /// <summary>
    /// 全局的 <see cref="System.Diagnostics.ActivitySource"/>
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new(DiagnosticConstants.ActivityNames.RootActivitySourceName);

    #endregion Public 属性
}
