namespace FluentWorkflow.Management.Web.UI;

/// <summary>
/// management ui options
/// </summary>
public class ManagementUIOptions
{
    #region Public 字段

    /// <summary>
    /// default api endpoint
    /// </summary>
    public const string DefaultApiEndpoint = $"{DefaultRoutePrefix}-api";

    /// <summary>
    /// default management entrypoint path
    /// </summary>
    public const string DefaultRoutePrefix = "/fwf";

    #endregion Public 字段

    #region Public 属性

    /// <summary>
    /// api endpoint.
    /// <br/><br/>when not set, use <see cref="DefaultApiEndpoint"/> as default item internally
    /// </summary>
    public string ApiEndpoint { get; set; } = DefaultApiEndpoint;

    /// <summary>
    /// Static resource HTTP cache time
    /// </summary>
    public TimeSpan? CacheLifetime { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// Options for enabling compression
    /// <br/><see langword="null"/> - compression will be disabled only in the development environment.
    /// <br/><see langword="true"/> - compression will always be enabled.
    /// <br/><see langword="false"/> - compression will always be disabled.
    /// <br/>default value is <see langword="null"/>
    /// </summary>
    public bool? CompressionEnabled { get; set; }

    /// <summary>
    /// management entrypoint path
    /// <br/>default with <see cref="DefaultRoutePrefix"/>
    /// </summary>
    public string RoutePrefix { get; set; } = DefaultRoutePrefix;

    #endregion Public 属性
}
