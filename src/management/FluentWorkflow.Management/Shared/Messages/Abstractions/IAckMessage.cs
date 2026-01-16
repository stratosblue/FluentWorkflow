namespace FluentWorkflow.Management.Shared.Messages.Abstractions;

/// <summary>
/// 确认消息
/// </summary>
public interface IAckMessage
{
    #region Public 属性

    /// <summary>
    /// 确认的消息的Id
    /// </summary>
    public int AckId { get; }

    /// <summary>
    /// 执行消息
    /// </summary>
    public string? ExecutionMessage { get; }

    /// <summary>
    /// 执行是否成功
    /// </summary>
    public bool IsExecutionSuccess { get; }

    #endregion Public 属性
}
