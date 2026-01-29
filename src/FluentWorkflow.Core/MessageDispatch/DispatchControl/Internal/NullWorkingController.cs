using System.Collections.Immutable;
using FluentWorkflow.Abstractions;

namespace FluentWorkflow.MessageDispatch.DispatchControl.Internal;

internal sealed class NullWorkingController : IWorkingController
{
    #region Public 事件

    /// <inheritdoc/>
    public event WorkingItemCreatedDelegate? WorkingItemCreated
    { add { } remove { } }

    /// <inheritdoc/>
    public event WorkingItemDisposingDelegate? WorkingItemDisposing
    { add { } remove { } }

    #endregion Public 事件

    #region Public 属性

    /// <summary>
    /// 静态共享实例
    /// </summary>
    public static NullWorkingController Shared { get; } = new();

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, WorkingItem> WorkingItems { get; } = ImmutableDictionary<string, WorkingItem>.Empty;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public ValueTask<IConsumptionControlScope> ConsumptionControlAsync<TMessage>(string eventName, TMessage message, CancellationToken cancellationToken) where TMessage : IWorkflowMessage
    {
        return NullConsumptionControlScope.CreateValueTask(cancellationToken);
    }

    #endregion Public 方法
}
