namespace FluentWorkflow.Management.Shared;

/// <summary>
/// 远程执行异常
/// </summary>
public class RemoteExecutionException : Exception
{
    #region Public 构造函数

    /// <inheritdoc cref="RemoteExecutionException"/>
    public RemoteExecutionException(string? message)
        : base(message ?? "Remote execution encountered an exception")
    {
    }

    /// <inheritdoc cref="RemoteExecutionException"/>
    public RemoteExecutionException(string? message, Exception inner)
        : base(message ?? "Remote execution encountered an exception", inner)
    {
    }

    #endregion Public 构造函数
}
