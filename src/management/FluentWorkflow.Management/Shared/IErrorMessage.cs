namespace FluentWorkflow.Management.Shared;

/// <summary>
/// 错误消息
/// </summary>
public interface IErrorMessage
{
    #region Public 属性

    /// <summary>
    /// 错误消息内容
    /// </summary>
    public string? Message { get; }

    #endregion Public 属性
}
