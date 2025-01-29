namespace FluentWorkflow.Interface;

/// <summary>
/// 子工作流程等待处理器 - 处理工作流程某阶段开启的子工作流程的等待逻辑
/// </summary>
public interface IWorkflowAwaitProcessor
{
    #region Public 方法

    /// <summary>
    /// 登记 <paramref name="finishedMessage"/> 对应的子工作流程已结束
    /// </summary>
    /// <param name="finishedMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>父流程的等待状态</returns>
    Task<WorkflowAwaitState> FinishedOneAsync(IWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// 登记父工作流程<paramref name="parentWorkflowContext"/>包含子工作流程<paramref name="childWorkflows"/>
    /// </summary>
    /// <param name="parentWorkflowContext"></param>
    /// <param name="childWorkflows"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RegisterAsync(IWorkflowContext parentWorkflowContext, IDictionary<string, IWorkflow> childWorkflows, CancellationToken cancellationToken = default);

    #endregion Public 方法
}

/// <summary>
/// 工作流程等待状态
/// </summary>
public sealed class WorkflowAwaitState
{
    #region Public 属性

    /// <summary>
    /// 子工作流程上下文
    /// </summary>
    public IReadOnlyDictionary<string, IWorkflowContext?> ChildWorkflowContexts { get; }

    /// <summary>
    /// 是否已结束
    /// </summary>
    public bool IsFinished { get; }

    /// <summary>
    /// 父工作流程上下文
    /// </summary>
    public WorkflowContextSnapshot ParentWorkflowContext { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="WorkflowAwaitState"/>
    /// </summary>
    /// <param name="parentWorkflowContext">父工作流程上下文</param>
    /// <param name="isFinished">是否已结束（失败或成功）</param>
    /// <param name="childWorkflowContexts">所有子工作流程的上下文字典</param>
    public WorkflowAwaitState(WorkflowContextSnapshot parentWorkflowContext, bool isFinished, IReadOnlyDictionary<string, IWorkflowContext?> childWorkflowContexts)
    {
        ParentWorkflowContext = parentWorkflowContext ?? throw new ArgumentNullException(nameof(parentWorkflowContext));
        IsFinished = isFinished;
        ChildWorkflowContexts = childWorkflowContexts ?? throw new ArgumentNullException(nameof(childWorkflowContexts));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 获取失败的上下文
    /// </summary>
    /// <param name="caredOnly">是否只获取在意完成状态的上下文</param>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<string, IWorkflowContext>> GetFailed(bool caredOnly = false)
    {
        foreach (var item in ChildWorkflowContexts)
        {
            if (item.Value?.IsFailed() == true)
            {
                if (caredOnly)
                {
                    if (!item.Value.Flag.HasFlag(WorkflowFlag.NotCareFinishState))
                    {
                        yield return item!;
                    }
                    continue;
                }
                yield return item!;
            }
        }
    }

    #endregion Public 方法
}
