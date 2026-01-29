namespace FluentWorkflow.MessageDispatch.DispatchControl;

/// <summary>
/// 消息分发上下文范围
/// </summary>
public interface IMessageDispatchContextScope : IDisposable
{
    #region Public 属性

    /// <summary>
    /// 范围的上下文
    /// </summary>
    public MessageDispatchContext Context { get; }

    #endregion Public 属性
}
