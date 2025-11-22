using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentWorkflow.Diagnostics;

namespace FluentWorkflow.Util;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class FluentWorkflowDebugUtil
{
    #region Private 字段

    private static readonly object s_lockRoot = new();

    private static ActivityListener? s_activityListener;

    private static ActivitySource? s_debugActivitySource;

    #endregion Private 字段

    #region Internal 属性

    internal static ActivitySource DebugActivitySource
    {
        get
        {
            if (s_debugActivitySource is { } debugActivitySource)
            {
                return debugActivitySource;
            }

            lock (s_lockRoot)
            {
                return s_debugActivitySource ??= new($"{DiagnosticConstants.ActivityNames.RootActivitySourceName}.Debug");
            }
        }
    }

    #endregion Internal 属性

    #region Public 方法

    /// <summary>
    /// 禁用调试<see cref="Activity"/>监听
    /// </summary>
    public static void DisableDebugActivityListen()
    {
        lock (s_lockRoot)
        {
            s_activityListener?.Dispose();
            s_activityListener = null;

            s_debugActivitySource?.Dispose();
            s_debugActivitySource = null;
        }
    }

    /// <summary>
    /// 启用调试<see cref="Activity"/>监听
    /// </summary>
    public static void EnableDebugActivityListen()
    {
        lock (s_lockRoot)
        {
            if (s_activityListener is null)
            {
                s_activityListener = Create();
                System.Diagnostics.ActivitySource.AddActivityListener(s_activityListener);
            }
        }
    }

    #endregion Public 方法

    #region Private 方法

    private static ActivityListener Create()
    {
        var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name.StartsWith(DiagnosticConstants.ActivityNames.RootActivitySourceName, StringComparison.Ordinal),
            SampleUsingParentId = (ref activityOptions) => ActivitySamplingResult.AllData,
            Sample = (ref activityOptions) => ActivitySamplingResult.AllData
        };

        return activityListener;
    }

    #endregion Private 方法
}
