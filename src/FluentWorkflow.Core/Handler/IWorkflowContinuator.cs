using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Handler;

/// <summary>
/// 工作流延续器
/// </summary>
public interface IWorkflowContinuator
{
    #region Public 方法

    /// <summary>
    /// 使用子工作流的完成消息<paramref name="childWorkflowFinishedMessage"/>延续其父工作流
    /// </summary>
    /// <param name="childWorkflowFinishedMessage">子工作流程的结束消息</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ContinueAsync(IWorkflowFinishedMessage childWorkflowFinishedMessage, CancellationToken cancellationToken = default);

    #endregion Public 方法
}
