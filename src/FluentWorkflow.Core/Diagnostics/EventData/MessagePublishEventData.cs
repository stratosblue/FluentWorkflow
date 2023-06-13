using FluentWorkflow.Interface;

namespace FluentWorkflow.Diagnostics.EventData;

/// <summary>
/// 事件数据 - <see cref="DiagnosticConstants.MessagePublish"/>
/// </summary>
public class MessagePublishEventData
{
    #region Public 属性

    /// <inheritdoc cref="IWorkflowMessage"/>
    public IWorkflowMessage Message { get; set; }

    #endregion Public 属性

    #region Internal 构造函数

    internal MessagePublishEventData(IWorkflowMessage message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    #endregion Internal 构造函数
}
