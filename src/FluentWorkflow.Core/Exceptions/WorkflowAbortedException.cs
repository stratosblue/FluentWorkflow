namespace FluentWorkflow;

/// <summary>
/// 工作流程中止异常
/// </summary>
public class WorkflowAbortedException : WorkflowException
{
    #region Public 属性

    /// <summary>
    /// 中止原因
    /// </summary>
    public object? AbortReason { get; }

    /// <summary>
    /// 关联的取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowAbortedException"/>
    public WorkflowAbortedException(object? abortReason)
        : this(null, abortReason, null, default)
    {
    }

    /// <inheritdoc cref="WorkflowAbortedException"/>
    public WorkflowAbortedException(object? abortReason, CancellationToken token)
        : this(null, abortReason, null, token)
    {
    }

    /// <inheritdoc cref="WorkflowAbortedException"/>
    public WorkflowAbortedException(object? abortReason, Exception? innerException, CancellationToken token)
        : this(null, abortReason, innerException, token)
    {
    }

    /// <inheritdoc cref="WorkflowAbortedException"/>
    public WorkflowAbortedException(string? message, object? abortReason, Exception? innerException, CancellationToken token)
        : base(message ?? CreateMessage(abortReason), innerException)
    {
        AbortReason = abortReason;
        CancellationToken = token;
    }

    #endregion Public 构造函数

    #region Private 方法

    private static string CreateMessage(object? abortReason)
    {
        return abortReason is null
               ? "Workflow aborted"
               : $"Workflow aborted: {abortReason}";
    }

    #endregion Private 方法
}
