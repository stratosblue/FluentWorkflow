using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.Management.Worker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.Worker;

internal class GetMessagesEndpoint : IStandardExportEndpoint<PagedResponseDto<MessageListDto>>
{
    #region Public 属性

    /// <inheritdoc/>
    public static string Summary { get; } = "获取工作者消息详情";

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public static RouteHandlerBuilder MapEndpoint(WebApplication app, RouteGroupBuilder builder)
    {
        return builder.MapGet("/messages", async (string appName,
                                                  Guid workerId,
                                                  ManagementManagerHub managerHub,
                                                  int page = 1,
                                                  int pageSize = 20,
                                                  CancellationToken cancellationToken = default) =>
        {
            var context = managerHub.GetWorker(appName, workerId);
            var descriptor = context.Descriptor;

            var request = new MessageListQueryRequest()
            {
                Page = page,
                PageSize = pageSize,
            };

            var messageQueryResponse = await context.CommunicationPipe.RequestAsync<MessageListQueryRequest, MessageListQueryResponse>(request, cancellationToken);

            var result = new PagedResponseDto<MessageListDto>
            {
                TotalCount = messageQueryResponse?.TotalCount,
                Items = messageQueryResponse?.Items.Select(m =>
                {
                    return new MessageListDto(m.Id, m.EventName, m.StartTime);
                }).ToList() ?? [],
            };

            return result;
        });
    }

    #endregion Public 方法
}

/// <summary>
/// 消息列表Dto
/// </summary>
/// <param name="Id">消息Id</param>
/// <param name="EventName">事件名称</param>
/// <param name="StartTime">开始处理时间</param>
public record MessageListDto(string Id, string EventName, DateTime StartTime);
