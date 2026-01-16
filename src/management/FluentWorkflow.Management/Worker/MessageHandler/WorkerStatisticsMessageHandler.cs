using FluentWorkflow.Management.Shared.MessageHandler;
using FluentWorkflow.Management.Shared.Messages;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Worker.MessageHandler;

internal class WorkerStatisticsMessageHandler([ServiceKey] string appName, WorkerStatistician workerStatistician, IServiceProvider serviceProvider)
    : GenericResponseMessageHandler<WorkerStatisticsRequest, WorkerStatistics>(appName, serviceProvider)
{
    #region Protected 方法

    protected override Task<WorkerStatistics> ProcessRequestAsync(IHoarwellContext context, WorkerStatisticsRequest input, CancellationToken cancellationToken)
    {
        var result = new WorkerStatistics()
        {
            MessageStatistics = CreateMessageStatistics(),
            ProcessingCount = workerStatistician.ProcessingCount,
        };
        return Task.FromResult(result);
    }

    #endregion Protected 方法

    #region Private 方法

    private IList<WorkerMessageTimeSequenceStatistics> CreateMessageStatistics()
    {
        var result = new WorkerMessageTimeSequenceStatistics()
        {
            TimeSpan = workerStatistician.StatisticalPeriod,
            IncomingCount = workerStatistician.IncomingMessageRecords.Count,
            CompletedCount = workerStatistician.CompletedMessageRecords.Count,
            AverageProcessingTimeSpan = TimeSpan.FromTicks((long)workerStatistician.CompletedMessageRecords.DefaultIfEmpty().Average(m => m.TimeSpan.Ticks)),
        };
        return [result];
    }

    #endregion Private 方法
}
