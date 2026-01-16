using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.Management.Worker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.Worker;

internal class GetMessageDetailEndpoint : IStandardExportEndpoint<PagedResponseDto<MessageDto>>
{
    #region Public 属性

    /// <inheritdoc/>
    public static string Summary { get; } = "获取工作者消息列表";

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public static RouteHandlerBuilder MapEndpoint(WebApplication app, RouteGroupBuilder builder)
    {
        return builder.MapGet("/message-detail", async (string appName,
                                                        Guid workerId,
                                                        ManagementManagerHub managerHub,
                                                        int page = 1,
                                                        int pageSize = 20,
                                                        string? eventName = null,
                                                        string? messageId = null,
                                                        CancellationToken cancellationToken = default) =>
        {
            var context = managerHub.GetWorker(appName, workerId);
            var descriptor = context.Descriptor;

            var request = new MessageQueryRequest()
            {
                Query = new()
                {
                    EventName = eventName,
                    MessageId = messageId,
                },
                Page = page,
                PageSize = pageSize,
            };

            var messageQueryResponse = await context.CommunicationPipe.RequestAsync<MessageQueryRequest, MessageQueryResponse>(request, cancellationToken);

            var result = new PagedResponseDto<MessageDto>
            {
                TotalCount = messageQueryResponse?.TotalCount,
                Items = messageQueryResponse?.Items.Select(m =>
                {
                    return new MessageDto(m.Id, m.EventName, m.StartTime, m.Metadata);
                }).ToList() ?? [],
            };

            return result;
        });
    }

    #endregion Public 方法
}

/// <summary>
/// 消息Dto
/// </summary>
/// <param name="Id">消息Id</param>
/// <param name="EventName">事件名称</param>
/// <param name="StartTime">开始处理时间</param>
/// <param name="Metadata">元数据</param>
public record MessageDto(string Id, string EventName, DateTime StartTime, IReadOnlyDictionary<string, string> Metadata);
