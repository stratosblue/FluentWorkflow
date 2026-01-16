namespace FluentWorkflow.Management.Shared;

/// <summary>
/// 资源未找到异常
/// </summary>
public class ResourceNotFoundException : Exception, IErrorMessage
{
    #region Public 构造函数

    /// <inheritdoc cref="ResourceNotFoundException"/>
    public ResourceNotFoundException(string? message) : base(message ?? "Resource not found")
    {
    }

    /// <summary>
    /// <inheritdoc cref="ResourceNotFoundException"/>
    /// </summary>
    /// <param name="resource">资源类别</param>
    /// <param name="resourceName">资源名称</param>
    public ResourceNotFoundException(string resource, string resourceName) : base($"Resource \"{resourceName}\" for \'{resource}\' not found")
    {
    }

    #endregion Public 构造函数
}
