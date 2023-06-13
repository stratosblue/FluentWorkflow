namespace FluentWorkflow.Interface;

/// <summary>
/// 工作流程启动器
/// </summary>
public interface IWorkflowStarter
{
    #region Public 属性

    /// <summary>
    /// 工作流程
    /// </summary>
    public IWorkflow Workflow { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 启动工作流程
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken = default);

    #endregion Public 方法
}
