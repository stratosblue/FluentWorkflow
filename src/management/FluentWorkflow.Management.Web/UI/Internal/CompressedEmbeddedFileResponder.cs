using System.Collections.Frozen;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace FluentWorkflow.Management.Web.UI.Internal;

internal sealed class CompressedEmbeddedFileResponder
{
    #region Private 字段

    private readonly Assembly _assembly;

    private readonly StringValues _cacheControlHeaderValue;

    private readonly bool _compressionEnabled;

    private readonly StringValues _gzipStringValues = "gzip";

    private readonly FrozenDictionary<string, ResourceIndexCache> _resourceIndexes;

    #endregion Private 字段

    #region Public 构造函数

    public CompressedEmbeddedFileResponder(Assembly assembly,
                                           string resourceNamePrefix,
                                           TimeSpan? cacheLifetime,
                                           bool compressionEnabled)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        _cacheControlHeaderValue = GetCacheControlHeaderValue(cacheLifetime);
        _compressionEnabled = compressionEnabled;

        var resourceNames = assembly.GetManifestResourceNames()
                                    .Where(name => name.StartsWith(resourceNamePrefix, StringComparison.Ordinal));

        //Because the resource file lacks /. Expand all path possibilities
        var resourceIndexes = new Dictionary<string, ResourceIndexCache>(StringComparer.Ordinal);
        var contentTypeProvider = new FileExtensionContentTypeProvider();

        var octetStreamContentType = "application/octet-stream";
        foreach (var name in resourceNames)
        {
            var pathName = name.Substring(resourceNamePrefix.Length);
            var contentType = contentTypeProvider.TryGetContentType(pathName, out var contentTypeValue)
                              ? contentTypeValue
                              : octetStreamContentType;

            var resourceIndexCache = new ResourceIndexCache(name, contentType);

            foreach (var path in ExpandPossiblePaths(pathName))
            {
                resourceIndexes.Add(path, resourceIndexCache);
            }
        }

        _resourceIndexes = resourceIndexes.ToFrozenDictionary(StringComparer.Ordinal);

        static IEnumerable<string> ExpandPossiblePaths(string name)
        {
            name = $"/{name.TrimStart('.')}";
            yield return name;

            var segments = name.Split('.');
            if (segments.Length > 2)
            {
                //TODO We don't need to implement this logic now
                throw new NotImplementedException();
            }
        }
    }

    #endregion Public 构造函数

    #region Public 方法

    public async Task<bool> TryRespondWithFileAsync(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value!;

        if (!_resourceIndexes.TryGetValue(path, out var resourceIndexCache))
        {
            return false;
        }

        var (etag, Length) = GetDecompressContentETag(resourceIndexCache);

        var ifNoneMatch = httpContext.Request.Headers.IfNoneMatch.ToString();
        if (ifNoneMatch == etag)
        {
            httpContext.Response.StatusCode = StatusCodes.Status304NotModified;
            return true;
        }

        var responseHeaders = httpContext.Response.Headers;

        var responseWithGZip = _compressionEnabled
                               && httpContext.IsGZipAccepted();

        if (responseWithGZip)
        {
            responseHeaders.ContentEncoding = _gzipStringValues;
        }

        responseHeaders.ContentType = resourceIndexCache.ContentType;
        responseHeaders.ETag = etag;
        responseHeaders.CacheControl = _cacheControlHeaderValue;

        using var stream = OpenResourceStream(resourceIndexCache);
        if (responseWithGZip)
        {
            responseHeaders.ContentLength = stream.Length;
            await stream.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted);
        }
        else
        {
            responseHeaders.ContentLength = Length;
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            await gzipStream.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted);
        }

        return true;
    }

    #endregion Public 方法

    #region Private 方法

    private static string GetCacheControlHeaderValue(TimeSpan? cacheLifetime)
    {
        if (cacheLifetime is { } maxAge)
        {
            return new CacheControlHeaderValue()
            {
                MaxAge = maxAge,
                Private = true,
            }.ToString();
        }
        else
        {
            return new CacheControlHeaderValue()
            {
                NoCache = true,
            }.ToString();
        }
    }

    private (StringValues ETag, long DecompressContentLength) GetDecompressContentETag(ResourceIndexCache resourceIndexCache)
    {
        //Get decompress content and hash
        if (resourceIndexCache.ETag.HasValue
            && resourceIndexCache.DecompressContentLength.HasValue)
        {
            return (resourceIndexCache.ETag.Value, resourceIndexCache.DecompressContentLength.Value);
        }

        using var stream = OpenResourceStream(resourceIndexCache);

        using var memoryStream = new MemoryStream((int)stream.Length * 2);
        using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
        gzipStream.CopyTo(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        resourceIndexCache.DecompressContentLength = memoryStream.Length;

        var hashData = MD5.HashData(memoryStream);

        resourceIndexCache.ETag = $"\"{Convert.ToBase64String(hashData)}\"";

        return (resourceIndexCache.ETag.Value, resourceIndexCache.DecompressContentLength.Value);
    }

    private Stream OpenResourceStream(ResourceIndexCache resourceIndexCache)
    {
        // Actually, since the name comes from GetManifestResourceNames(), the content can definitely be obtained
        return _assembly.GetManifestResourceStream(resourceIndexCache.ResourceName)!;
    }

    #endregion Private 方法

    #region Private 类

    [DebuggerDisplay("{ResourceName,nq} [{ContentType,nq}]")]
    private sealed class ResourceIndexCache(string resourceName, string contentType)
    {
        #region Public 属性

        public StringValues ContentType { get; } = contentType ?? throw new ArgumentNullException(nameof(contentType));

        public long? DecompressContentLength { get; set; }

        public StringValues? ETag { get; set; }

        public string ResourceName { get; } = resourceName ?? throw new ArgumentNullException(nameof(resourceName));

        #endregion Public 属性
    }

    #endregion Private 类
}
