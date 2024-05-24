using FluentWorkflow.Interface;

namespace FluentWorkflow.Diagnostics.EventData;

/// <summary>
/// 事件数据 - <see cref="DiagnosticConstants.MessageReceived"/>
/// </summary>
public class MessageReceivedEventData
{
    #region Public 属性

    /// <inheritdoc cref="IWorkflowMessage"/>
    public object Message { get; set; }

    #endregion Public 属性

    #region Internal 构造函数

    internal MessageReceivedEventData(object message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    #endregion Internal 构造函数
}
