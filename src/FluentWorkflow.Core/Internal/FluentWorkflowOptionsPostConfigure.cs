using FluentWorkflow;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="FluentWorkflowOptions"/> PostConfigure
/// </summary>
public abstract class FluentWorkflowOptionsPostConfigure
    : IPostConfigureOptions<FluentWorkflowOptions>
{
    #region Public 方法

    /// <inheritdoc/>
    public void PostConfigure(string? name, FluentWorkflowOptions options)
    {
        if (string.IsNullOrEmpty(name))
        {
            PostConfigure(options);
        }
    }

    #endregion Public 方法

    #region Protected 方法

    /// <inheritdoc cref="PostConfigure(string?, FluentWorkflowOptions)"/>
    protected abstract void PostConfigure(FluentWorkflowOptions options);

    #endregion Protected 方法
}
