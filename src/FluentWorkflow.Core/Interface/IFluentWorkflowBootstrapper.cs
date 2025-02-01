namespace FluentWorkflow;

/// <summary>
/// FluentWorkflow 引导程序
/// </summary>
public interface IFluentWorkflowBootstrapper : IAsyncDisposable
{
    #region Public 方法

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitAsync(CancellationToken cancellationToken);

    #endregion Public 方法
}
