using FluentWorkflow.Abstractions;

namespace FluentWorkflow.MessageDispatch.DispatchControl;

/// <summary>
/// 工作控制器
/// </summary>
public interface IWorkingController
{
    #region Public 事件

    /// <summary>
    /// 工作项创建事件
    /// </summary>
    event WorkingItemCreatedDelegate? WorkingItemCreated;

    /// <summary>
    /// 工作项处置事件
    /// </summary>
    event WorkingItemDisposingDelegate? WorkingItemDisposing;

    #endregion Public 事件

    #region Public 属性

    /// <summary>
    /// 工作项字典
    /// </summary>
    IReadOnlyDictionary<string, WorkingItem> WorkingItems { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 创建一个针对<paramref name="message"/>的消费控制范围
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="eventName"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<IConsumptionControlScope> ConsumptionControlAsync<TMessage>(string eventName, TMessage message, CancellationToken cancellationToken) where TMessage : IWorkflowMessage;

    #endregion Public 方法
}

#region 委托

/// <summary>
/// 工作项创建回调委托
/// </summary>
/// <param name="workingItem">工作项</param>
public delegate void WorkingItemCreatedDelegate(WorkingItem workingItem);

/// <summary>
/// 工作项处置回调委托
/// </summary>
/// <param name="workingItem">工作项</param>
public delegate void WorkingItemDisposingDelegate(WorkingItem workingItem);

#endregion 委托

/// <summary>
/// 工作项
/// </summary>
/// <param name="EventName">事件名称</param>
/// <param name="Message">消息</param>
/// <param name="Metadata">消息调度元数据</param>
/// <param name="ControlScope">控制范围</param>
/// <param name="StartTime">开始时间</param>
public record WorkingItem(string EventName, IWorkflowMessage Message, MessageDispatchMetadata Metadata, IConsumptionControlScope ControlScope, DateTime StartTime)
{
};
