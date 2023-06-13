using System.Runtime.CompilerServices;
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
    public WorkflowException(string message, Exception inner) : base(message, inner)
    {
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="WorkflowException"/>
    protected WorkflowException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数

    #region Public 方法

    /// <summary>
    ///
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="paramName"></param>
    public static void ThrowIfNullOrWhiteSpace(string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("Can not be null or whitespace.", paramName);
        }
    }

    #endregion Public 方法
}
