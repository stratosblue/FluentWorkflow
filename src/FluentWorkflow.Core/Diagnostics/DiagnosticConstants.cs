using System.Diagnostics;

namespace FluentWorkflow.Diagnostics;

/// <summary>
/// 诊断常量
/// </summary>
public static class DiagnosticConstants
{
    #region Public 字段

    /// <summary>
    /// 诊断名称
    /// </summary>
    public const string DiagnosticName = "FluentWorkflowDiagnostic";

    /// <summary>
    /// 消息 - 处理结束
    /// </summary>
    public const string MessageHandleFinished = $"{DiagnosticName}.Message.Handle.Finished";

    /// <summary>
    /// 消息 - 发送
    /// </summary>
    public const string MessagePublish = $"{DiagnosticName}.Message.Publish";

    /// <summary>
    /// 消息 - 发送
    /// </summary>
    public const string MessageReceived = $"{DiagnosticName}.Message.Received";

    /// <summary>
    /// 阶段消息处理 - 结束
    /// </summary>
    public const string StageMessageHandleEnd = $"{DiagnosticName}.StageMessage.Handle.End";

    /// <summary>
    /// 阶段消息处理 - 开始
    /// </summary>
    public const string StageMessageHandleStart = $"{DiagnosticName}.StageMessage.Handle.Start";

    /// <summary>
    /// 工作流程调度 - 开始
    /// </summary>
    public const string WorkflowScheduleStart = $"{DiagnosticName}.Schedule.Start";

    /// <summary>
    /// 工作流程阶段完成
    /// </summary>
    public const string WorkflowStageCompleted = $"{DiagnosticName}.Stage.Completed";

    #endregion Public 字段

    #region Public 类

    /// <summary>
    /// <see cref="Activity"/> 名称
    /// </summary>
    public static class ActivityNames
    {
        #region Public 字段

        /// <summary>
        /// <see cref="System.Diagnostics.ActivitySource"/> 名称
        /// </summary>
        public const string RootActivitySourceName = FluentWorkflowConstants.ActivitySourceName;

        /// <summary>
        /// 阶段推进
        /// </summary>
        public const string StageMoving = $"{RootActivitySourceName}.StageMoving";

        /// <summary>
        /// 阶段处理
        /// </summary>
        public const string StageProcessing = $"{RootActivitySourceName}.StageProcessing";

        /// <summary>
        /// 工作流程启动
        /// </summary>
        public const string WorkflowStarting = $"{RootActivitySourceName}.WorkflowStarting";

        #endregion Public 字段

        #region Public 类

        /// <summary>
        /// Tag key
        /// </summary>
        public static class TagKeys
        {
            #region Public 字段

            /// <summary>
            /// 上下文
            /// </summary>
            public const string Context = "workflow.context";

            /// <summary>
            /// 失败消息
            /// </summary>
            public const string FailureMessage = "workflow.failure_msg";

            /// <summary>
            /// 消息
            /// </summary>
            public const string Message = "workflow.msg";

            /// <summary>
            /// 阶段状态
            /// </summary>
            public const string StageState = "workflow.stage.state";

            #endregion Public 字段
        }

        #endregion Public 类
    }

    #endregion Public 类
}
