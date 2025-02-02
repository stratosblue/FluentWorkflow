#pragma warning disable CS0618
#pragma warning disable CS8618

using System.Runtime.Serialization;

namespace FluentWorkflow;

/// <summary>
/// 工作流程失败异常
/// </summary>
[Serializable]
public class WorkflowFailureException : WorkflowException
{
    #region Private 字段

    private readonly string? _remoteStackTrace;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 失败时的上下文
    /// </summary>
    public IWorkflowContext Context { get; }

    /// <summary>
    /// 流程ID
    /// </summary>
    public string Id { get; }

    /// <inheritdoc/>
    public override string? StackTrace
    {
        get
        {
            if (string.IsNullOrEmpty(_remoteStackTrace))
            {
                return base.StackTrace;
            }
            return $"[Remote] {_remoteStackTrace} [Local] {base.StackTrace}";
        }
    }

    /// <summary>
    /// 阶段
    /// </summary>
    public string Stage { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowFailureException"/>
    public WorkflowFailureException(string id, string stage, string message, string? remoteStackTrace, IWorkflowContext context) : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(stage);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        ArgumentNullException.ThrowIfNull(context);
        Id = id;
        Stage = stage;
        _remoteStackTrace = remoteStackTrace;
        Context = context;
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="WorkflowFailureException"/>
    protected WorkflowFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数
}
