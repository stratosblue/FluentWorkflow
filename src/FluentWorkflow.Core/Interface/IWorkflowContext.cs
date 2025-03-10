﻿using System.ComponentModel;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow;

/// <summary>
/// 工作流程上下文
/// </summary>
public interface IWorkflowContext
    : IUniqueId
    , ICurrentStage
{
    #region Public 属性

    /// <summary>
    /// 工作流程标识
    /// </summary>
    public WorkflowFlag Flag { get; [EditorBrowsable(EditorBrowsableState.Advanced)] set; }

    /// <summary>
    /// 元数据
    /// </summary>
    public WorkflowContextMetadata Metadata { get; }

    /// <summary>
    /// 父工作流程上下文
    /// </summary>
    public WorkflowContextSnapshot? Parent { get; }

    /// <summary>
    /// 状态
    /// </summary>
    public WorkflowContextState State { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 应用修改 (<paramref name="snapshotContext"/> 应当是由当前上下文的快照创建的新上下文)
    /// </summary>
    /// <param name="snapshotContext"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void ApplyChanges(IWorkflowContext snapshotContext);

    /// <summary>
    /// 获取上下文数据快照
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<string, string> GetSnapshot();

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public TValue? GetValue<TValue>(string key);

    /// <summary>
    /// 设置上下文当前阶段
    /// </summary>
    /// <param name="stage"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCurrentStage(string stage);

    /// <summary>
    /// 设置父工作流程上下文
    /// </summary>
    /// <param name="parent"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetParent(WorkflowContextSnapshot parent);

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue<TValue>(string key, TValue? value);

    #endregion Public 方法
}
