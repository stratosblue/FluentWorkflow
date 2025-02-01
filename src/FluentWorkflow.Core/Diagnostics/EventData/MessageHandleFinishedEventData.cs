using FluentWorkflow.Abstractions;

namespace FluentWorkflow.Diagnostics.EventData;

/// <summary>
/// 事件数据 - <see cref="DiagnosticConstants.MessageHandleFinished"/>
/// </summary>
public class MessageHandleFinishedEventData
{
    #region Public 属性

    /// <summary>
    /// 处理过程中出现的异常
    /// </summary>
    public Exception? Exception { get; }

    /// <inheritdoc cref="IWorkflowMessage"/>
    public object Message { get; set; }

    #endregion Public 属性

    #region Internal 构造函数

    internal MessageHandleFinishedEventData(object message, Exception? exception)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
    }

    #endregion Internal 构造函数
}
