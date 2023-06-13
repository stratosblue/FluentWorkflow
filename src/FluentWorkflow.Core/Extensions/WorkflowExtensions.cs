using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
///
/// </summary>
public static class WorkflowExtensions
{
    #region Public 方法

    /// <summary>
    /// 是否已失败
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <returns></returns>
    public static bool IsFailed(this IWorkflowContext workflowContext)
    {
        return !string.IsNullOrEmpty(workflowContext.GetValue(FluentWorkflowConstants.ContextKeys.FailureStage));
    }

    /// <summary>
    /// 是否已启动
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>
    public static bool IsStarted(this IWorkflow workflow) => workflow.Context.IsStarted();

    /// <summary>
    /// 是否已启动
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <returns></returns>
    public static bool IsStarted(this IWorkflowContext workflowContext)
    {
        return !string.IsNullOrEmpty(workflowContext.Stage);
    }

    #endregion Public 方法
}
