namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程阶段完成器
/// </summary>
public interface IWorkflowStageFinalizer
{
    #region Public 方法

    /// <summary>
    /// 子工作流程等待结束
    /// </summary>
    /// <param name="context"></param>
    /// <param name="childWorkflowContexts"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AwaitFinishedAsync(IWorkflowContext context, IReadOnlyDictionary<string, IWorkflowContext?> childWorkflowContexts, CancellationToken cancellationToken = default);

    /// <summary>
    /// 完成阶段
    /// </summary>
    /// <param name="context">要完成的阶段上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CompleteAsync(IWorkflowContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// 阶段失败
    /// </summary>
    /// <param name="context">要失败的阶段上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task FailAsync(IWorkflowContext context, CancellationToken cancellationToken = default);

    #endregion Public 方法
}
