namespace FluentWorkflow.MessageDispatch.DispatchControl;

/// <summary>
/// 消费控制域
/// </summary>
public interface IConsumptionControlScope : IAsyncDisposable
{
    #region Public 属性

    /// <summary>
    /// 关联控制域的令牌
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// 控制的原因
    /// </summary>
    object? ControlReason { get; }

    /// <summary>
    /// 控制的类型
    /// </summary>
    ConsumptionControlType ControlType { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 如果控制范围存在中止，则包裹 <paramref name="exception"/> 后重新抛出异常
    /// </summary>
    /// <param name="exception"></param>
    /// <exception cref="WorkflowAbortedException"></exception>
    /// <exception cref="RunningWorkEvictedException"></exception>
    void TryThrowWithControlException(Exception exception);

    #endregion Public 方法
}
