using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FluentWorkflow.Extensions;

/// <summary>
/// <see cref="IDisposable"/> 拓展方法
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class FluentWorkflowIDisposableExtensions
{
    #region Public 方法

    /// <summary>
    /// 如果 <paramref name="disposable"/> 是 <see cref="IAsyncDisposable"/> 则使用异步处置，否则使用同步处置
    /// </summary>
    /// <param name="disposable"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask DisposeAsync<TDisposable>(this TDisposable? disposable) where TDisposable : IDisposable
    {
        if (disposable is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        disposable?.Dispose();
        return ValueTask.CompletedTask;
    }

    #endregion Public 方法
}
