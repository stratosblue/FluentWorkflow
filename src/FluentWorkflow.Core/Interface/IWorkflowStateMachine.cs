namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程状态机
/// </summary>
public interface IWorkflowStateMachine
{
    #region Public 属性

    /// <summary>
    /// 工作流程上下文
    /// </summary>
    public IWorkflowContext Context { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 检查是否完成
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> IsCompletedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 推进状态
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task MoveNextAsync(CancellationToken cancellationToken = default);

    #endregion Public 方法
}
