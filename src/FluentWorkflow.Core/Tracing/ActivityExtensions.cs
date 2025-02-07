using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FluentWorkflow.Tracing;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ActivityExtensions
{
    #region Public 方法

    /// <summary>
    /// 批量添加Baggage
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="baggages"></param>
    /// <returns></returns>
    public static Activity AddBaggages(this Activity activity, IEnumerable<KeyValuePair<string, string?>> baggages)
    {
        foreach (var (key, value) in baggages)
        {
            activity.AddBaggage(key, value);
        }
        return activity;
    }

    /// <summary>
    /// 在任务 <paramref name="task"/> 完成时处理 <paramref name="activity"/>
    /// </summary>
    /// <param name="task"></param>
    /// <param name="activity"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void DisposeActivityWhenTaskCompleted(this Task task, Activity? activity)
    {
        if (activity is null)
        {
            return;
        }

        task.ContinueWith(RecordTaskExceptionAndDisposeActivity, activity, CancellationToken.None);
    }

    /// <summary>
    /// 记录异常异常
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="exception"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Activity RecordException(this Activity activity, Exception exception, DateTimeOffset? timestamp = default)
    {
        activity.RecordExceptionEvent(exception, timestamp);
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        return activity;
    }

    /// <summary>
    /// 批量添加Baggage
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="baggages"></param>
    /// <returns></returns>
    public static Activity SetBaggages(this Activity activity, IEnumerable<KeyValuePair<string, string?>> baggages)
    {
        foreach (var (key, value) in baggages)
        {
            activity.SetBaggage(key, value);
        }
        return activity;
    }

    #region AddEvent

    /// <summary>
    /// 添加 <see cref="ActivityEvent"/>
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static Activity AddEvent([NotNullWhen(true)] this Activity activity, string name)
    {
        return activity.AddEvent(new ActivityEvent(name));
    }

    #endregion AddEvent

    #endregion Public 方法

    #region Private 方法

    private static void RecordExceptionEvent(this Activity activity, Exception exception, DateTimeOffset? timestamp = default)
    {
        var tagsCollection = new ActivityTagsCollection
        {
            { "exception.type", exception.GetType().FullName },
            { "exception.stacktrace", exception.StackTrace },
        };

        if (!string.IsNullOrWhiteSpace(exception.Message))
        {
            tagsCollection.Add("exception.message", exception.Message);
        }

        activity.AddEvent(new ActivityEvent("exception", timestamp ?? DateTimeOffset.UtcNow, tagsCollection));
    }

    private static void RecordTaskExceptionAndDisposeActivity(Task task, object? state)
    {
        if (state is Activity stateActivity)
        {
            if (task.Exception is not null)
            {
                stateActivity.RecordException(task.Exception);
            }

            stateActivity.Dispose();
        }
    }

    #endregion Private 方法
}
