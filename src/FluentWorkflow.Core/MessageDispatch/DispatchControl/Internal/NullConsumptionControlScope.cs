namespace FluentWorkflow.MessageDispatch.DispatchControl.Internal;

internal sealed class NullConsumptionControlScope(CancellationToken cancellationToken) : IConsumptionControlScope
{
    #region Public 属性

    /// <summary>
    /// 静态共享实例
    /// </summary>
    public static NullConsumptionControlScope Shared { get; } = new(CancellationToken.None);

    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; } = cancellationToken;

    /// <inheritdoc/>
    public object? ControlReason { get; }

    /// <inheritdoc/>
    public ConsumptionControlType ControlType => ConsumptionControlType.Undefined;

    #endregion Public 属性

    #region Public 方法

    public static ValueTask<IConsumptionControlScope> CreateValueTask(CancellationToken cancellationToken)
    {
        return cancellationToken.CanBeCanceled || cancellationToken.IsCancellationRequested
               ? ValueTask.FromResult<IConsumptionControlScope>(new NullConsumptionControlScope(cancellationToken))
               : ValueTask.FromResult<IConsumptionControlScope>(Shared);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => default;

    /// <inheritdoc/>
    public void TryThrowWithControlException(Exception exception)
    {
    }

    #endregion Public 方法
}
