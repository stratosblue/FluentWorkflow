using System.Text.Json.Serialization;
using FluentWorkflow.Management.Web.Endpoints;
using FluentWorkflow.Management.Web.Endpoints.App;
using FluentWorkflow.Management.Web.Endpoints.Status;
using FluentWorkflow.Management.Web.Endpoints.Worker;

namespace FluentWorkflow.Management.Web;

[JsonSerializable(typeof(StandardApiResponse<object>))]
[JsonSerializable(typeof(ConsumptionControlDto))]
[JsonSerializable(typeof(StandardApiResponse<bool>))]
[JsonSerializable(typeof(StandardApiResponse<OverviewDto>))]
[JsonSerializable(typeof(StandardApiResponse<WorkerDetailDto>))]
[JsonSerializable(typeof(StandardApiResponse<PagedResponseDto<WorkerStatusDto>>))]
[JsonSerializable(typeof(StandardApiResponse<PagingRequestDto>))]
[JsonSerializable(typeof(StandardApiResponse<PagedResponseDto<AppsDetailDto>>))]
[JsonSerializable(typeof(StandardApiResponse<PagedResponseDto<MessageListDto>>))]
[JsonSerializable(typeof(StandardApiResponse<PagedResponseDto<MessageDto>>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

