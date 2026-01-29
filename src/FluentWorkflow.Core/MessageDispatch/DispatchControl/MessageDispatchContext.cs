using System.ComponentModel;

namespace FluentWorkflow.MessageDispatch.DispatchControl;

/// <summary>
/// 消息调度上下文
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed class MessageDispatchContext
{
    #region Private 字段

    private static readonly AsyncLocal<MessageDispatchContext?> s_asyncLocal = new();

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 当前值
    /// </summary>
    public static MessageDispatchContext? Current => s_asyncLocal.Value;

    /// <summary>
    /// 元数据
    /// </summary>
    public MessageDispatchMetadata Metadata { get; } = new(null);

    /// <summary>
    /// 开启一个范围
    /// </summary>
    /// <returns></returns>
    public static IMessageDispatchContextScope BeginScope()
    {
        var preContext = Current;
        var context = new MessageDispatchContext();
        s_asyncLocal.Value = context;
        return new MessageDispatchContextScope(preContext, context);
    }

    #endregion Public 属性

    #region Private 类

    private sealed class MessageDispatchContextScope(MessageDispatchContext? preContext, MessageDispatchContext context) : IMessageDispatchContextScope
    {
        #region Private 字段

        private int _isDisposed = 0;

        #endregion Private 字段

        #region Public 属性

        public MessageDispatchContext Context => context;

        #endregion Public 属性

        #region Public 方法

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                s_asyncLocal.Value = preContext;
            }
        }

        #endregion Public 方法
    }

    #endregion Private 类
}
