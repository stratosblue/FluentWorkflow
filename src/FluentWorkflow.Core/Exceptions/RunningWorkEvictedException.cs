using FluentWorkflow.MessageDispatch;

namespace FluentWorkflow;

/// <summary>
/// 运行中的工作被驱逐异常
/// </summary>
public class RunningWorkEvictedException : WorkflowException, IBusyConsumer
{
    #region Public 属性

    /// <summary>
    /// 关联的取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// 驱逐原因
    /// </summary>
    public object? EvictionReason { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="RunningWorkEvictedException"/>
    public RunningWorkEvictedException(object? evictionReason)
        : this(null, evictionReason, null, default)
    {
    }

    /// <inheritdoc cref="RunningWorkEvictedException"/>
    public RunningWorkEvictedException(object? abortReason, CancellationToken token)
        : this(null, abortReason, null, token)
    {
    }

    /// <inheritdoc cref="RunningWorkEvictedException"/>
    public RunningWorkEvictedException(object? abortReason, Exception? innerException, CancellationToken token)
        : this(null, abortReason, innerException, token)
    {
    }

    /// <inheritdoc cref="RunningWorkEvictedException"/>
    public RunningWorkEvictedException(string? message, object? evictionReason, Exception? innerException, CancellationToken token)
        : base(message ?? CreateMessage(evictionReason), innerException)
    {
        EvictionReason = evictionReason;
        CancellationToken = token;
    }

    #endregion Public 构造函数

    #region Private 方法

    private static string CreateMessage(object? evictionReason)
    {
        return evictionReason is null
               ? "Running work evicted"
               : $"Running work evicted: {evictionReason}";
    }

    #endregion Private 方法
}
