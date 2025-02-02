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
        /// 失败信息
        /// </summary>
        public const string FailureInformation = "-x-context-failure-information";

        /// <summary>
        /// 上下文发送信息，记录上下文的流转记录<br/>
        /// 其值正确情况下应当形如<br/>
        /// xx@host1, xx@host2, xx@host1, ...
        /// </summary>
        public const string Forwarded = "-x-context-forwarded";

        /// <summary>
        /// 上下文元数据
        /// </summary>
        public const string Metadata = "-x-context-metadata";

        /// <summary>
        /// 父工作流程
        /// </summary>
        public const string ParentWorkflow = "-x-context-parent-workflow";

        /// <summary>
        /// 上下文状态
        /// </summary>
        public const string State = "-x-context-state";

        #endregion Public 字段

        #region Public 方法

        /// <summary>
        /// <paramref name="key"/>是否是仅初始化Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsInitOnlyKey(string key)
        {
            return string.Equals(Metadata, key)
                   || string.Equals(State, key)
                   || string.Equals(ParentWorkflow, key);
        }

        #endregion Public 方法
    }

    #endregion Public 类
}
