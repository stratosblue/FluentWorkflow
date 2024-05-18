using System.ComponentModel;

namespace FluentWorkflow.Interface;

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
    public WorkflowFlag Flag { get; set; }

    /// <summary>
    /// 父工作流程上下文
    /// </summary>
    public WorkflowContextMetadata? Parent { get; }

    #endregion Public 属性

    #region Public 方法

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
    public string? GetValue(string key);

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
    public void SetParent(WorkflowContextMetadata parent);

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(string key, string? value);

    #endregion Public 方法
}
