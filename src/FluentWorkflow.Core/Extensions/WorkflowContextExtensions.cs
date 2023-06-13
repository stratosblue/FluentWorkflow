using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Interface;

namespace FluentWorkflow.Extensions;

/// <summary>
///
/// </summary>
public static class WorkflowContextExtensions
{
    #region Public 方法

    #region WorkflowAlias

    /// <summary>
    /// 获取子工作流程别名
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <returns></returns>
    public static string GetChildWorkflowAlias(this IWorkflowContext workflowContext)
    {
        var alias = workflowContext.GetValue(FluentWorkflowConstants.ContextKeys.WorkflowAlias)
                    ?? throw new WorkflowInvalidOperationException($"The context not contains the alias key \"{FluentWorkflowConstants.ContextKeys.WorkflowAlias}\".");

        return alias;
    }

    /// <summary>
    /// 设置子工作流程别名
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    public static void SetChildWorkflowAlias(this IWorkflowContext workflowContext, string alias)
    {
        workflowContext.SetValue(FluentWorkflowConstants.ContextKeys.WorkflowAlias, alias);
    }

    /// <summary>
    /// 尝试获取子工作流程别名
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    public static bool TryGetChildWorkflowAlias(this IWorkflowContext workflowContext, [NotNullWhen(true)] out string? alias)
    {
        alias = workflowContext.GetValue(FluentWorkflowConstants.ContextKeys.WorkflowAlias);
        return alias != null;
    }

    #endregion WorkflowAlias

    #region FailureStackTrace

    /// <summary>
    /// 设置失败栈追踪
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <param name="failureStackTrace"></param>
    public static void SetFailureStackTrace(this IWorkflowContext workflowContext, string? failureStackTrace)
    {
        workflowContext.SetValue(FluentWorkflowConstants.ContextKeys.FailureStackTrace, failureStackTrace);
    }

    /// <summary>
    /// 尝试获取失败栈追踪
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <param name="failureStackTrace"></param>
    /// <returns></returns>
    public static bool TryGetFailureStackTrace(this IWorkflowContext workflowContext, [NotNullWhen(true)] out string? failureStackTrace)
    {
        failureStackTrace = workflowContext.GetValue(FluentWorkflowConstants.ContextKeys.FailureStackTrace);
        return failureStackTrace != null;
    }

    #endregion FailureStackTrace

    #region FailureMessage

    /// <summary>
    /// 设置失败栈追踪
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <param name="failureMessage"></param>
    public static void SetFailureMessage(this IWorkflowContext workflowContext, string failureMessage)
    {
        workflowContext.SetValue(FluentWorkflowConstants.ContextKeys.FailureMessage, failureMessage);
    }

    /// <summary>
    /// 尝试获取失败消息
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <param name="failureMessage"></param>
    /// <returns></returns>
    public static bool TryGetFailureMessage(this IWorkflowContext workflowContext, [NotNullWhen(true)] out string? failureMessage)
    {
        failureMessage = workflowContext.GetValue(FluentWorkflowConstants.ContextKeys.FailureMessage);
        return failureMessage != null;
    }

    #endregion FailureMessage

    #endregion Public 方法
}
