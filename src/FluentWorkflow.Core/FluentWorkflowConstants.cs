namespace FluentWorkflow;

/// <summary>
/// 工作流程相关常量定义
/// </summary>
public static class FluentWorkflowConstants
{
    #region Public 字段

    /// <summary>
    /// ActivitySource 名称
    /// </summary>
    public const string ActivitySourceName = $"{nameof(FluentWorkflow)}";

    /// <summary>
    /// 默认的消费者Logger名称
    /// </summary>
    public const string DefaultConsumerLoggerName = $"{nameof(FluentWorkflow)}.EventMessageConsumer";

    #endregion Public 字段

    #region Public 类

    /// <summary>
    /// 上下文 Key
    /// </summary>
    public static class ContextKeys
    {
        #region Public 字段

        /// <summary>
        /// 失败消息
        /// </summary>
        public const string FailureMessage = "-x-context-failure-message";

        /// <summary>
        /// 失败栈追踪
        /// </summary>
        public const string FailureStackTrace = "-x-context-failure-stack-trace";

        /// <summary>
        /// 失败阶段
        /// </summary>
        public const string FailureStage = "-x-context-failure-stage";

        /// <summary>
        /// Id
        /// </summary>
        public const string Id = "-x-context-id";

        /// <summary>
        /// 父追踪上下文
        /// </summary>
        public const string ParentTraceContext = "-x-parent-trace-context";

        /// <summary>
        /// 父工作流程
        /// </summary>
        public const string ParentWorkflow = "-x-context-parent-workflow";

        /// <summary>
        /// 当前阶段
        /// </summary>
        public const string Stage = "-x-context-stage";

        /// <summary>
        /// 工作流程别名
        /// </summary>
        public const string WorkflowAlias = "-x-workflow-alias";

        /// <summary>
        /// 工作流程标识
        /// </summary>
        public const string WorkflowFlag = "-x-context-flag";

        /// <summary>
        /// 工作流程名称
        /// </summary>
        public const string WorkflowName = "-x-workflow-name";

        #endregion Public 字段

        #region Public 方法

        /// <summary>
        /// <paramref name="key"/>是否是仅初始化Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsInitOnlyKey(string key)
        {
            return string.Equals(WorkflowName, key)
                   || string.Equals(Id, key)
                   || string.Equals(Stage, key)
                   || string.Equals(ParentWorkflow, key);
        }

        #endregion Public 方法
    }

    #endregion Public 类
}
