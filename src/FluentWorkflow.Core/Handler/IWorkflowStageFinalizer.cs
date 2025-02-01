namespace FluentWorkflow.Handler;

/// <summary>
/// 工作流程阶段完成器<br/>
/// 进行工作流程某个阶段完成时的处理
/// </summary>
public interface IWorkflowStageFinalizer
{
    #region Public 方法

    /// <summary>
    /// <paramref name="context"/> 对应的工作流所有子工作流程已等待结束
    /// </summary>
    /// <param name="context"></param>
    /// <param name="childWorkflowContexts"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AwaitFinishedAsync(IWorkflowContext context, IReadOnlyDictionary<string, IWorkflowContext?> childWorkflowContexts, CancellationToken cancellationToken = default);

    /// <summary>
    /// 完成 <paramref name="context"/> 对应的工作流的当前阶段
    /// </summary>
    /// <param name="context">要完成的阶段上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CompleteAsync(IWorkflowContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// 进行 <paramref name="context"/> 对应的工作流的当前阶段失败处理
    /// </summary>
    /// <param name="context">要失败的阶段上下文</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task FailAsync(IWorkflowContext context, CancellationToken cancellationToken = default);

    #endregion Public 方法
}
