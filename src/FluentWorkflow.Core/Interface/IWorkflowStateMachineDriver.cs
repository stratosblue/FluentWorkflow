namespace FluentWorkflow.Interface;

/// <summary>
/// <typeparamref name="TWorkflow"/> 状态机驱动器
/// </summary>
/// <typeparam name="TWorkflow">工作流程</typeparam>
/// <typeparam name="TStageCompletedMessage">阶段完成消息</typeparam>
/// <typeparam name="TFailureMessage">失败消息</typeparam>
public interface IWorkflowStateMachineDriver<in TWorkflow, in TStageCompletedMessage, in TFailureMessage>
    where TWorkflow : IWorkflow
    where TStageCompletedMessage : IWorkflowStageCompletedMessage
    where TFailureMessage : IWorkflowFailureMessage
{
    #region Public 方法

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TStageCompletedMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TFailureMessage message, CancellationToken cancellationToken = default);

    #endregion Public 方法
}
