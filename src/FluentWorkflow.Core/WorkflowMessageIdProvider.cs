using System.ComponentModel;

namespace FluentWorkflow;

/// <summary>
/// 工作流程消息ID提供器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WorkflowMessageIdProvider
{
    #region Public 方法

    /// <summary>
    /// 生成一个新ID
    /// </summary>
    /// <returns></returns>
    public static string Generate()
    {
#if NET9_0_OR_GREATER
        return Guid.CreateVersion7().ToString("N");
#else
        return Guid.NewGuid().ToString("N");
#endif
    }

    #endregion Public 方法
}
