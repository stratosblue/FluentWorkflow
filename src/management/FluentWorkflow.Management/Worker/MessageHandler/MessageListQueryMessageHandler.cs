using FluentWorkflow.Management.Shared.MessageHandler;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.MessageDispatch.DispatchControl;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Worker.MessageHandler;

internal class MessageListQueryMessageHandler([ServiceKey] string appName, IWorkingController workingController, IServiceProvider serviceProvider)
    : GenericResponseMessageHandler<MessageListQueryRequest, MessageListQueryResponse>(appName, serviceProvider)
{
    #region Protected 方法

    protected override Task<MessageListQueryResponse> ProcessRequestAsync(IHoarwellContext context, MessageListQueryRequest input, CancellationToken cancellationToken)
    {
        var queryResult = InternalQuery(input);

        var result = new MessageListQueryResponse()
        {
            TotalCount = queryResult.TotalCount,
            Items = queryResult.Items.Select(CreateDetail).ToList(),
        };
        return Task.FromResult(result);

        static MessageBasicInfo CreateDetail(WorkingItem workingItem)
        {
            return new()
            {
                Id = workingItem.Message.Id,
                EventName = workingItem.EventName,
                StartTime = workingItem.StartTime
            };
        }
    }

    #endregion Protected 方法

    #region Private 方法

    private QueryResult InternalQuery(MessageListQueryRequest input)
    {
        var workingItems = workingController.WorkingItems;

        if (input.Query is { } query)
        {
            if (query.EventName is { } eventName)
            {
                var filteredItems = workingItems.Values.Where(m => m.EventName == eventName).OrderBy(m => m.StartTime).ToList();

                return new(filteredItems.Count, [.. filteredItems.Skip(input.Skip).Take(input.PageSize)]);
            }
        }

        var all = workingItems.Values.OrderBy(m => m.StartTime).ToList();

        return new(all.Count, [.. all.Skip(input.Skip).Take(input.PageSize)]);
    }

    #endregion Private 方法

    private record QueryResult(int? TotalCount, IList<WorkingItem> Items)
    {
        public static QueryResult Empty { get; } = new(0, Array.Empty<WorkingItem>());
    };
}
