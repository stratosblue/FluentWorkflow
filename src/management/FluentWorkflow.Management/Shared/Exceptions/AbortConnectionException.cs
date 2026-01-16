namespace FluentWorkflow.Management.Shared;

/// <summary>
/// 中断连接异常
/// </summary>
public class AbortConnectionException : Exception
{
    #region Public 构造函数

    /// <inheritdoc cref="AbortConnectionException"/>
    public AbortConnectionException()
    { }

    /// <inheritdoc cref="AbortConnectionException"/>
    public AbortConnectionException(string message) : base(message)
    {
    }

    /// <inheritdoc cref="AbortConnectionException"/>
    public AbortConnectionException(string message, Exception inner) : base(message, inner)
    {
    }

    #endregion Public 构造函数
}
