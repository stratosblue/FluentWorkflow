using FluentWorkflow.Interface;

namespace FluentWorkflow.Diagnostics.EventData;

/// <summary>
/// 事件数据 - <see cref="DiagnosticConstants.WorkflowScheduleStart"/>
/// </summary>
public class WorkflowScheduleStartEventData
{
    #region Public 属性

    /// <inheritdoc cref="IWorkflow"/>
    public IWorkflow Workflow { get; set; }

    #endregion Public 属性

    #region Internal 构造函数

    internal WorkflowScheduleStartEventData(IWorkflow workflow)
    {
        Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
    }

    #endregion Internal 构造函数
}
