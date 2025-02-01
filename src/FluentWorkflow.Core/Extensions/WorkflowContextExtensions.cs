using System.ComponentModel;

namespace FluentWorkflow.Extensions;

/// <summary>
///
/// </summary>
public static class WorkflowContextExtensions
{
    #region Public 方法

    /// <summary>
    /// 获取失败信息
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <returns></returns>
    public static WorkflowFailureInformation? GetFailureInformation(this IWorkflowContext workflowContext)
    {
        return workflowContext.GetValue<WorkflowFailureInformation>(FluentWorkflowConstants.ContextKeys.FailureInformation);
    }

    /// <summary>
    /// 设置失败信息
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <param name="stage"></param>
    /// <param name="message"></param>
    /// <param name="stackTrace"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetFailureInformation(this IWorkflowContext workflowContext, string stage, string message, string? stackTrace)
    {
        ArgumentException.ThrowIfNullOrEmpty(stage);
        ArgumentException.ThrowIfNullOrEmpty(message);

        var failureInformation = new WorkflowFailureInformation(stage, message, stackTrace);
        workflowContext.SetValue(FluentWorkflowConstants.ContextKeys.FailureInformation, failureInformation);
    }

    #region Forwarded

    /// <summary>
    /// 添加上下文流转信息 <see cref="FluentWorkflowConstants.ContextKeys.Forwarded"/>
    /// </summary>
    /// <param name="workflowContext"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void AppendForwarded(this IWorkflowContext workflowContext)
    {
        var forwardeds = workflowContext.GetValue<List<string>>(FluentWorkflowConstants.ContextKeys.Forwarded);

        if (forwardeds is null)
        {
            workflowContext.SetValue<List<string>>(FluentWorkflowConstants.ContextKeys.Forwarded, [FluentWorkflowEnvironment.Description]);
        }
        else
        {
            forwardeds.Add(FluentWorkflowEnvironment.Description);
        }
    }

    #endregion Forwarded

    #endregion Public 方法
}
