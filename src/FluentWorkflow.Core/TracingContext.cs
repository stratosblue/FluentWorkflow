using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace FluentWorkflow.Tracing;

/// <summary>
/// 追踪上下文
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct TracingContext
{
    #region Public 属性

    /// <summary>
    /// 行李
    /// </summary>
    [JsonConverter(typeof(ActivityBaggageJsonConverter))]
    public IEnumerable<KeyValuePair<string, string?>> Baggage { get; }

    /// <summary>
    /// SpanId
    /// </summary>
    public string SpanId { get; }

    /// <summary>
    /// TraceFlags
    /// </summary>
    public ActivityTraceFlags TraceFlags { get; }

    /// <summary>
    /// TraceId
    /// </summary>
    public string TraceId { get; }

    /// <summary>
    /// TraceState
    /// </summary>
    public string? TraceState { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="TracingContext"/>
    public TracingContext(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        var context = activity.Context;
        TraceId = context.TraceId.ToHexString();
        SpanId = context.SpanId.ToHexString();
        Baggage = activity.Baggage.Reverse().ToList();  //HACK 当前baggage遍历时为从后往前，在此处反向排序，以保证符合添加顺序
        TraceFlags = context.TraceFlags;
        TraceState = context.TraceState;
    }

    /// <inheritdoc cref="TracingContext"/>
    [JsonConstructor]
    public TracingContext(string traceId, string spanId, IEnumerable<KeyValuePair<string, string?>> baggage, ActivityTraceFlags traceFlags, string? traceState)
    {
        Baggage = baggage;
        SpanId = spanId;
        TraceFlags = traceFlags;
        TraceId = traceId;
        TraceState = traceState;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 获取类型化的 SpanId
    /// </summary>
    /// <returns></returns>
    public ActivitySpanId GetSpanId() => ActivitySpanId.CreateFromString(SpanId);

    /// <summary>
    /// 获取类型化的 TraceId
    /// </summary>
    /// <returns></returns>
    public ActivityTraceId GetTraceId() => ActivityTraceId.CreateFromString(TraceId);

    /// <summary>
    /// 从描述还原一个上下文
    /// </summary>
    /// <param name="isRemote"></param>
    /// <returns></returns>
    public ActivityContext RestoreActivityContext(bool isRemote) => new(GetTraceId(), GetSpanId(), TraceFlags, TraceState, isRemote);

    #endregion Public 方法

    #region Util

    /// <summary>
    /// 捕获当前上下文
    /// </summary>
    /// <returns></returns>
    public static TracingContext Capture() => Activity.Current is { } activity ? new(activity) : throw new InvalidOperationException("There is currently no tracing.");

    /// <summary>
    /// 创建上下文
    /// </summary>
    /// <param name="activity"></param>
    /// <returns></returns>
    public static TracingContext Create(Activity activity) => new(activity);

    #endregion Util
}
