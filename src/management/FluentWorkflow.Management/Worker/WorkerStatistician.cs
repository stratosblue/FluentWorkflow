using System.Collections.Concurrent;
using FluentWorkflow.MessageDispatch.DispatchControl;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow.Management.Worker;

/// <summary>
/// Worker统计器
/// </summary>
internal sealed class WorkerStatistician : IDisposable
{
    #region Private 字段

    private readonly CancellationTokenSource _cleanCTS = new();

    private readonly ConcurrentQueue<CompletedMessageRecord> _completedMessageRecords = new();

    private readonly ConcurrentQueue<MessageRecord> _incomingMessageRecords = new();

    private readonly ILogger _logger;

    private readonly IWorkingController _workingController;

    private bool _isDisposed;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 已完成消息记录
    /// </summary>
    public IReadOnlyCollection<CompletedMessageRecord> CompletedMessageRecords => _completedMessageRecords;

    /// <summary>
    /// 传入消息记录
    /// </summary>
    public IReadOnlyCollection<MessageRecord> IncomingMessageRecords => _incomingMessageRecords;

    /// <summary>
    /// 处理中的数量
    /// </summary>
    public int ProcessingCount => _workingController.WorkingItems.Count;

    /// <summary>
    /// 统计的时段
    /// </summary>
    public TimeSpan StatisticalPeriod { get; } = TimeSpan.FromMinutes(10);

    #endregion Public 属性

    #region Public 构造函数

    public WorkerStatistician(IWorkingController workingController, ILogger<WorkerStatistician> logger)
    {
        _workingController = workingController ?? throw new ArgumentNullException(nameof(workingController));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        workingController.WorkingItemCreated += WorkingItemCreated;
        workingController.WorkingItemDisposing += WorkingItemDisposing;

        var token = _cleanCTS.Token;

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), token);
                try
                {
                    var threshold = DateTime.UtcNow - StatisticalPeriod;

                    while (_incomingMessageRecords.TryPeek(out var messageRecord)
                           && messageRecord.MessageTime < threshold)
                    {
                        _incomingMessageRecords.TryDequeue(out _);
                    }

                    while (_completedMessageRecords.TryPeek(out var completedMessageRecord)
                           && completedMessageRecord.RecordTime < threshold)
                    {
                        _completedMessageRecords.TryDequeue(out _);
                    }
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();
                    logger.LogWarning(ex, "Error while clean statistics.");
                }
            }
        }, token);
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _workingController.WorkingItemCreated -= WorkingItemCreated;
        _workingController.WorkingItemDisposing -= WorkingItemDisposing;

        _cleanCTS.Cancel();

        _cleanCTS.Dispose();

        _incomingMessageRecords.Clear();
        _completedMessageRecords.Clear();
    }

    #endregion Public 方法

    #region Private 方法

    private void WorkingItemCreated(WorkingItem workingItem)
    {
        try
        {
            _incomingMessageRecords.Enqueue(new(workingItem.StartTime));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while add statistics.");
        }
    }

    private void WorkingItemDisposing(WorkingItem workingItem)
    {
        try
        {
            var now = DateTime.UtcNow;
            var startTime = workingItem.StartTime;
            _completedMessageRecords.Enqueue(new(now, startTime, now - startTime));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while add statistics.");
        }
    }

    #endregion Private 方法

    /// <summary>
    /// 消息记录
    /// </summary>
    /// <param name="MessageTime">消息时间</param>
    public record struct MessageRecord(DateTime MessageTime);

    /// <summary>
    /// 完成消息记录
    /// </summary>
    /// <param name="RecordTime">记录时间</param>
    /// <param name="MessageTime">开始时间</param>
    /// <param name="TimeSpan">处理时长</param>
    public record struct CompletedMessageRecord(DateTime RecordTime, DateTime MessageTime, TimeSpan TimeSpan);
}
