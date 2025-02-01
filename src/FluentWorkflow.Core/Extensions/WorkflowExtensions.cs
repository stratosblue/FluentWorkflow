using FluentWorkflow.Abstractions;

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
        return workflowContext.GetValue<WorkflowFailureInformation>(FluentWorkflowConstants.ContextKeys.FailureInformation) is not null;
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
