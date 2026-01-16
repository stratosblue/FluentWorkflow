using FluentWorkflow.Management;
using FluentWorkflow.Management.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow.Management.Web.Endpoints;

internal class StandardEndpointFilter<TResponse>(ILogger<StandardEndpointFilter<TResponse>> logger)
    : IEndpointFilter
{
    #region Public 方法

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            var result = await next(context);
            return StandardApiResponse.Success<TResponse>((TResponse?)result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error at http request {Context}", context);
            return StandardApiResponse.Error((ex as IErrorMessage)?.Message);
        }
    }

    #endregion Public 方法
}
