namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程调度器
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
public interface IWorkflowScheduler<in TWorkflow>
    where TWorkflow : IWorkflow
{
    #region Public 方法

    /// <summary>
    /// 启动工作流程
    /// </summary>
    /// <param name="workflow"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(TWorkflow workflow, CancellationToken cancellationToken = default);

    #endregion Public 方法
}
