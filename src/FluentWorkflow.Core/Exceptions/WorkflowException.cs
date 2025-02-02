using System.Runtime.Serialization;

namespace FluentWorkflow;

/// <summary>
/// 工作流程异常
/// </summary>
[Serializable]
public class WorkflowException : Exception
{
    #region Public 构造函数

    /// <inheritdoc cref="WorkflowException"/>
    public WorkflowException(string message) : base(message)
    {
    }

    /// <inheritdoc cref="WorkflowException"/>
    public WorkflowException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="WorkflowException"/>
    [Obsolete("see https://github.com/dotnet/docs/issues/34893")]
    protected WorkflowException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数
}
