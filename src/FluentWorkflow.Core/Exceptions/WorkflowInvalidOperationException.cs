﻿using System.Runtime.Serialization;

namespace FluentWorkflow;

/// <summary>
/// 工作流程无效操作异常
/// </summary>
[Serializable]
public class WorkflowInvalidOperationException : WorkflowException
{
    #region Public 构造函数

    /// <inheritdoc cref="WorkflowInvalidOperationException"/>
    public WorkflowInvalidOperationException(string message) : base(message)
    {
    }

    /// <inheritdoc cref="WorkflowInvalidOperationException"/>
    public WorkflowInvalidOperationException(string message, Exception inner) : base(message, inner)
    {
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="WorkflowInvalidOperationException"/>
    [Obsolete("see https://github.com/dotnet/docs/issues/34893")]
    protected WorkflowInvalidOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数
}
