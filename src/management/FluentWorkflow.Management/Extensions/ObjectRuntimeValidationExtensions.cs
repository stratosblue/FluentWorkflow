#pragma warning disable IDE0079 // 请删除不必要的忽略

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.ComponentModel.RuntimeValidation;

/// <summary>
/// 对象运行时检查拓展
/// </summary>
internal static class ObjectRuntimeValidationExtensions
{
    /// <summary>
    /// 要求 <paramref name="target"/> 必须非 <see langword="null"/>，否则抛出 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="targetExpression"></param>
    /// <returns><paramref name="target"/> 的值</returns>
    [return: NotNull]
    [DebuggerStepThrough]
    public static T Required<T>([NotNull] this T? target, [CallerArgumentExpression(nameof(target))] string? targetExpression = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(target, targetExpression);

        return target;
    }

    /// <summary>
    /// 要求 <paramref name="target"/> 必须非 <see langword="null"/>，否则抛出 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="targetExpression"></param>
    /// <returns><paramref name="target"/> 的值</returns>
    [DebuggerStepThrough]
    public static T Required<T>([NotNull] this in T? target, [CallerArgumentExpression(nameof(target))] string? targetExpression = null) where T : struct
    {
        if (!target.HasValue)
        {
            throw new ArgumentNullException(targetExpression);
        }

        return target.Value;
    }

    #region Async

#pragma warning disable ECS1004 // 异步方法应当具有显式的 System.Threading.CancellationToken 参数

    #region Task

    /// <summary>
    /// 要求 <paramref name="targetTask"/> 的返回值必须非 <see langword="null"/>，否则抛出 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetTask"></param>
    /// <param name="targetTaskExpression"></param>
    /// <returns><paramref name="targetTask"/> 的值</returns>
    [DebuggerStepThrough]
    public static async Task<T> RequiredResult<T>(this Task<T?> targetTask, [CallerArgumentExpression(nameof(targetTask))] string? targetTaskExpression = null) where T : class
    {
        var targetTaskResult = await targetTask;

        ArgumentNullException.ThrowIfNull(targetTaskResult, $"{targetTaskExpression}.Result");

        return targetTaskResult;
    }

    /// <summary>
    /// 要求 <paramref name="targetTask"/> 的返回值必须非 <see langword="null"/>，否则抛出 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetTask"></param>
    /// <param name="targetTaskExpression"></param>
    /// <returns><paramref name="targetTask"/> 的值</returns>
    [DebuggerStepThrough]
    public static async Task<T> RequiredResult<T>(this Task<T?> targetTask, [CallerArgumentExpression(nameof(targetTask))] string? targetTaskExpression = null) where T : struct
    {
        var targetTaskResult = await targetTask;

        if (!targetTaskResult.HasValue)
        {
            throw new ArgumentNullException($"{targetTaskExpression}.Result");
        }

        return targetTaskResult.Value;
    }

    #endregion Task

    #region ValueTask

    /// <summary>
    /// 要求 <paramref name="targetTask"/> 的返回值必须非 <see langword="null"/>，否则抛出 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetTask"></param>
    /// <param name="targetTaskExpression"></param>
    /// <returns><paramref name="targetTask"/> 的值</returns>
    [DebuggerStepThrough]
    public static async ValueTask<T> RequiredResult<T>(this ValueTask<T?> targetTask, [CallerArgumentExpression(nameof(targetTask))] string? targetTaskExpression = null) where T : class
    {
        var targetTaskResult = await targetTask;

        ArgumentNullException.ThrowIfNull(targetTaskResult, $"{targetTaskExpression}.Result");

        return targetTaskResult;
    }

    /// <summary>
    /// 要求 <paramref name="targetTask"/> 的返回值必须非 <see langword="null"/>，否则抛出 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetTask"></param>
    /// <param name="targetTaskExpression"></param>
    /// <returns><paramref name="targetTask"/> 的值</returns>
    [DebuggerStepThrough]
    public static async ValueTask<T> RequiredResult<T>([NotNull] this ValueTask<T?> targetTask, [CallerArgumentExpression(nameof(targetTask))] string? targetTaskExpression = null) where T : struct
    {
        var targetTaskResult = await targetTask;

        if (!targetTaskResult.HasValue)
        {
            throw new ArgumentNullException($"{targetTaskExpression}.Result");
        }

        return targetTaskResult.Value;
    }

    #endregion ValueTask

#pragma warning restore ECS1004 // 异步方法应当具有显式的 System.Threading.CancellationToken 参数

    #endregion Async
}
