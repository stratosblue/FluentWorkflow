using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FluentWorkflow;

public class InMemoryWorkflowFinishWaiter
{
    #region Public 属性

    public TaskCompletionSource CompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    #endregion Public 属性

    public InMemoryWorkflowFinishWaiter()
    {
        //避免测试资源管理器警告
        CompletionSource.Task.ContinueWith(static task => task.Exception?.Flatten(), TaskContinuationOptions.OnlyOnFaulted);
    }

    #region Public 方法

    public void SetException(Exception exception)
    {
        CompletionSource.SetException(exception);
    }

    public void SetResult()
    {
        CompletionSource.SetResult();
    }

    public Task WaitAsync(TimeSpan? timeout = null)
    {
        if (Debugger.IsAttached)    //附加调试器时无限等待
        {
            return CompletionSource.Task.WaitAsync(Timeout.InfiniteTimeSpan);
        }

        return CompletionSource.Task.WaitAsync(timeout ?? TimeSpan.FromSeconds(5));
    }

    #endregion Public 方法
}

public class InMemoryWorkflowFinishWaiterContainer
{
    #region Private 字段

    private readonly ConcurrentDictionary<string, InMemoryWorkflowFinishWaiter> _allWaiters = new();

    #endregion Private 字段

    #region Public 索引器

    public InMemoryWorkflowFinishWaiter this[string id]
    {
        [return: NotNull]
        get => _allWaiters.GetOrAdd(id, _ => new());
        set
        {
            if (value is null)
            {
                _allWaiters.TryRemove(id, out _);
            }
            else
            {
                _allWaiters[id] = value;
            }
        }
    }

    #endregion Public 索引器
}
