namespace FluentWorkflow.MessageDispatch.DispatchControl;

/// <summary>
/// 消费控制类型
/// </summary>
public enum ConsumptionControlType
{
    /// <summary>
    /// 未定义
    /// </summary>
    Undefined,

    /// <summary>
    /// 中止工作流程
    /// </summary>
    AbortWorkflow,

    /// <summary>
    /// 驱逐运行中的工作
    /// </summary>
    EvictRunningWork,
}
