using System.Text;
using FluentWorkflow.Management.Web.UI.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace FluentWorkflow.Management.Web.UI;

internal sealed class ManagementUIMiddleware
{
    #region Private 字段

    private const string EmbeddedFileNamespace = "FluentWorkflow.Management.Web.dist";

    private readonly CompressedEmbeddedFileResponder _compressedEmbeddedFileResponder;

    private readonly ReadOnlyMemory<byte> _configurationJsonData;

    private readonly StringValues _javascriptContentsType = "text/javascript";

    private readonly RequestDelegate _next;

    #endregion Private 字段

    #region Public 构造函数

    public ManagementUIMiddleware(RequestDelegate next, ManagementUIOptions options, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(options);

        _next = next;
        var compressionEnabled = options.CompressionEnabled is null
                                 ? !hostEnvironment.IsDevelopment()
                                 : options.CompressionEnabled.Value;

        var assembly = typeof(ManagementUIMiddleware).Assembly;
        _compressedEmbeddedFileResponder = new(assembly: assembly,
                                               resourceNamePrefix: EmbeddedFileNamespace,
                                               cacheLifetime: options.CacheLifetime,
                                               compressionEnabled: compressionEnabled);

        var cconfiguration = $$"""
        {
          "baseApiAddress": "{{options.ApiEndpoint}}"
        }
        """;

        _configurationJsonData = Encoding.UTF8.GetBytes(cconfiguration);
    }

    #endregion Public 构造函数

    #region Public 方法

    public async Task Invoke(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value;

        //static resources
        if (!await _compressedEmbeddedFileResponder.TryRespondWithFileAsync(httpContext))
        {
            if (string.IsNullOrEmpty(path)
                || string.Equals(path, "/", StringComparison.Ordinal))
            {
                httpContext.Response.StatusCode = StatusCodes.Status302Found;
                httpContext.Response.Headers.Location = $"{httpContext.Request.PathBase}/index.html";
                return;
            }

            if (string.Equals("/configuration.json", path, StringComparison.Ordinal))
            {
                httpContext.Response.Headers.ContentLength = _configurationJsonData.Length;
                httpContext.Response.Headers.ContentType = _javascriptContentsType;

                await httpContext.Response.BodyWriter.WriteAsync(_configurationJsonData, httpContext.RequestAborted);
                return;
            }

            await _next(httpContext);
        }
    }

    #endregion Public 方法
}
