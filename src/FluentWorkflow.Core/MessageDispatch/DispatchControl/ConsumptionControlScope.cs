namespace FluentWorkflow.MessageDispatch.DispatchControl;

/// <summary>
/// 消费控制范围
/// </summary>
internal sealed class ConsumptionControlScope : IConsumptionControlScope
{
    #region Private 字段

    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly Action _disposeCallback;

    private int _isDisposed = 0;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; }

    /// <inheritdoc/>
    public object? ControlReason { get; private set; }

    /// <inheritdoc/>
    public ConsumptionControlType ControlType { get; private set; }

    #endregion Public 属性

    #region Public 构造函数

    public ConsumptionControlScope(Action disposeCallback, CancellationToken associatedCancellationToken)
    {
        ArgumentNullException.ThrowIfNull(disposeCallback);

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(associatedCancellationToken);
        CancellationToken = _cancellationTokenSource.Token;
        _disposeCallback = disposeCallback;
    }

    #endregion Public 构造函数

    #region Private 析构函数

    ~ConsumptionControlScope()
    {
        Task.Run(async () =>
        {
            await DisposeAsync();
        });
    }

    #endregion Private 析构函数

    #region Public 方法

    /// <inheritdoc/>
    public void AbortWorkflow(object? reason) => InternalCancel(ConsumptionControlType.AbortWorkflow, reason);

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
        {
            _cancellationTokenSource.Dispose();
            _disposeCallback();
        }
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public void EvictRunningWork(object? reason) => InternalCancel(ConsumptionControlType.EvictRunningWork, reason);

    /// <inheritdoc/>
    public void TryThrowWithControlException(Exception exception)
    {
        if (CancellationToken.IsCancellationRequested)
        {
            throw ControlType switch
            {
                ConsumptionControlType.EvictRunningWork => new RunningWorkEvictedException(ControlReason, exception, CancellationToken),
                _ => new WorkflowAbortedException(ControlReason, exception, CancellationToken),
            };
        }
    }

    #endregion Public 方法

    #region Private 方法

    private void InternalCancel(ConsumptionControlType type, object? reason)
    {
        ObjectDisposedException.ThrowIf(_isDisposed != 0, this);

        ControlType = type;
        ControlReason = reason;

        _cancellationTokenSource.Cancel();
    }

    #endregion Private 方法
}
