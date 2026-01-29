using System.Collections.Concurrent;
using System.Diagnostics;
using FluentWorkflow.Abstractions;
using FluentWorkflow.MessageDispatch.DispatchControl.Internal;

namespace FluentWorkflow.MessageDispatch.DispatchControl;

/// <summary>
/// 默认的 <inheritdoc cref="IWorkingController"/>
/// </summary>
public class DefaultWorkingController : IWorkingController, IDisposable
{
    #region Private 字段

    private readonly ConcurrentDictionary<string, WorkingItem> _workingItems = new(StringComparer.Ordinal);

    private bool _isDisposed;

    #endregion Private 字段

    #region Public 事件

    /// <inheritdoc/>
    public event WorkingItemCreatedDelegate? WorkingItemCreated;

    /// <inheritdoc/>
    public event WorkingItemDisposingDelegate? WorkingItemDisposing;

    #endregion Public 事件

    #region Public 属性

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, WorkingItem> WorkingItems => _workingItems;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public virtual ValueTask<IConsumptionControlScope> ConsumptionControlAsync<TMessage>(string eventName, TMessage message, CancellationToken cancellationToken)
        where TMessage : IWorkflowMessage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(message);

        var context = MessageDispatchContext.Current;

        if (context?.Metadata is { } metadata)
        {
            if (Activity.Current is { } activity)
            {
                metadata.Set(activity.TraceId.ToHexString(), "-x-trace-id");
                metadata.Set(activity.SpanId.ToHexString(), "-x-span-id");
            }
        }

        ConsumptionControlScope scope = null!;
        WorkingItem? workingItem = null;

        scope = new ConsumptionControlScope(() =>
        {
            if (workingItem != null
                && !_isDisposed)
            {
                OnWorkingItemDisposing(workingItem);
            }
        }, cancellationToken);

        workingItem = new WorkingItem(EventName: eventName,
                                      Message: message,
                                      Metadata: context?.Metadata ?? new(),
                                      ControlScope: scope,
                                      StartTime: DateTime.UtcNow);

        if (_workingItems.TryAdd(message.Id, workingItem))
        {
            OnWorkingItemCreated(workingItem);
            return ValueTask.FromResult<IConsumptionControlScope>(scope);
        }

        workingItem = null;

        _ = Task.Run(async () =>
        {
            await scope.DisposeAsync();
        }, CancellationToken.None);

        return NullConsumptionControlScope.CreateValueTask(cancellationToken);
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 在工作项被创建时
    /// </summary>
    protected virtual void OnWorkingItemCreated(WorkingItem workingItem)
    {
        WorkingItemCreated?.Invoke(workingItem);
    }

    /// <summary>
    /// 在工作项被处置时
    /// </summary>
    protected virtual void OnWorkingItemDisposing(WorkingItem workingItem)
    {
        var id = workingItem.Message.Id;
        if (_workingItems.TryRemove(id, out var removed)
            && !ReferenceEquals(removed, workingItem))
        {
            //移除了其它项，正确使用时不应该会这样
            _workingItems.TryAdd(id, removed);
        }

        WorkingItemDisposing?.Invoke(workingItem);
    }

    #endregion Protected 方法

    #region Dispose

    /// <summary>
    ///
    /// </summary>
    ~DefaultWorkingController()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _workingItems.Clear();
            }

            _isDisposed = true;
        }
    }

    #endregion Dispose
}
