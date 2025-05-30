﻿using System.Diagnostics.CodeAnalysis;

namespace FluentWorkflow.Handler;

/// <summary>
/// 工作流延续器中心<br/>
/// 用于检查工作流某阶段是否存在延续器 <see cref="IWorkflowContinuator"/>，并获取其实例
/// </summary>
public interface IWorkflowContinuatorHub
{
    #region Public 方法

    /// <summary>
    /// 检查是否有延续器
    /// </summary>
    /// <param name="workflowName"></param>
    /// <param name="stageName"></param>
    /// <returns></returns>
    bool HasContinuator(string workflowName, string stageName);

    /// <summary>
    /// 尝试获取指定的工作流延续器
    /// </summary>
    /// <param name="workflowName"></param>
    /// <param name="stageName"></param>
    /// <param name="workflowContinuator"></param>
    /// <returns></returns>
    bool TryGet(string workflowName, string stageName, [NotNullWhen(true)] out IWorkflowContinuator? workflowContinuator);

    #endregion Public 方法
}
