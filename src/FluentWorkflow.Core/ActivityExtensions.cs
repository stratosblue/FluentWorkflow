using System.ComponentModel;
using System.Diagnostics;

namespace FluentWorkflow.Tracing;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ActivityExtensions
{
    #region Public 方法

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

    #endregion Private 方法
}
