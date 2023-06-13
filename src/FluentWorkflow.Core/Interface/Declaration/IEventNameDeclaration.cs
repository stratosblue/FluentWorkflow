namespace FluentWorkflow.Interface;

/// <summary>
/// 事件名称声明
/// </summary>
public interface IEventNameDeclaration
{
    #region Public 属性

    /// <summary>
    /// 事件名称
    /// </summary>
    public abstract static string EventName { get; }

    #endregion Public 属性
}
