using FluentWorkflow.Management.Manager;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.Management.Worker;
using FluentWorkflow.MessageDispatch.DispatchControl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FluentWorkflow.Management.Web.Endpoints.Worker;

internal class ConsumptionControlEndpoint : IStandardExportEndpoint<bool>
{
    #region Public 属性

    /// <inheritdoc/>
    public static string Summary { get; } = "消费控制";

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public static RouteHandlerBuilder MapEndpoint(WebApplication app, RouteGroupBuilder builder)
    {
        return builder.MapPost("/consumption-control", static async (ConsumptionControlDto input,
                                                              ManagementManagerHub managerHub,
                                                              CancellationToken cancellationToken = default) =>
        {
            var (appName, workerId, messageId, type, reason) = input;

            var context = managerHub.GetWorker(appName, workerId);

            var consumptionControl = new ConsumptionControl()
            {
                TargetMessageId = messageId,
                Type = type switch
                {
                    ConsumptionControlType.EvictRunningWork or ConsumptionControlType.AbortWorkflow => type,
                    _ => throw new ArgumentException($"Unsupported type {type:G}"),
                },
                Reason = reason ?? $"{type:G} by management",
            };

            var result = await context.CommunicationPipe.RequestAsync<ConsumptionControl, ConsumptionControlResult>(consumptionControl, cancellationToken);

            return result?.IsSuccess == true;
        });
    }

    #endregion Public 方法
}

/// <summary>
/// 消费控制Dto
/// </summary>
/// <param name="AppName">应用程序名称</param>
/// <param name="WorkerId">工作者Id</param>
/// <param name="MessageId">消息Id</param>
/// <param name="Type">控制类型</param>
/// <param name="Reason">控制原因描述</param>
public record class ConsumptionControlDto(string AppName,
                                          Guid WorkerId,
                                          string MessageId,
                                          ConsumptionControlType Type,
                                          string? Reason = null);
