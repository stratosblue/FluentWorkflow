namespace FluentWorkflow.Management.Shared.Messages.Abstractions;

/// <summary>
/// 消息
/// </summary>
public interface IMessage
{
    #region Public 属性

    /// <summary>
    /// 消息的Id
    /// </summary>
    public int Id { get; }

    #endregion Public 属性
}
