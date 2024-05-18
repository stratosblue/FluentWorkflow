namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程 <typeparamref name="TWorkflow"/> 的恢复器<br/>
/// 用于恢复被挂起的流程上下文，使其继续执行后续流程
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
public interface IWorkflowResumer<out TWorkflow> where TWorkflow : IWorkflow
{
    #region Public 方法

    /// <summary>
    /// 恢复被挂起的流程 <paramref name="context"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ResumeAsync(IWorkflowContext context, CancellationToken cancellationToken = default);

    #endregion Public 方法
}
