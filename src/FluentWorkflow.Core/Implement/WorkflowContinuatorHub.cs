using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentWorkflow;

/// <summary>
/// 工作流延续器中心
/// </summary>
public class WorkflowContinuatorHub : IWorkflowContinuatorHub
{
    #region Private 字段

    private readonly ImmutableDictionary<string, ImmutableDictionary<string, Type>> _continuatorTypes;

    private readonly IServiceProvider _serviceProvider;

    private readonly IServiceProviderIsService _serviceProviderIsService;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowContinuatorHub"/>
    public WorkflowContinuatorHub(IOptions<FluentWorkflowOptions> optionsAccessor, IServiceProvider serviceProvider, IServiceProviderIsService serviceProviderIsService)
    {
        if (optionsAccessor?.Value is null)
        {
            throw new ArgumentNullException(nameof(optionsAccessor));
        }
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _serviceProviderIsService = serviceProviderIsService ?? throw new ArgumentNullException(nameof(serviceProviderIsService));

        var options = optionsAccessor.Value;
        _continuatorTypes = options.Continuators.ToImmutableDictionary(m => m.Key, m => m.Value.ToImmutableDictionary(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public bool HasContinuator(string workflowName, string stageName)
    {
        WorkflowException.ThrowIfNullOrWhiteSpace(workflowName);
        WorkflowException.ThrowIfNullOrWhiteSpace(stageName);

        return _continuatorTypes.TryGetValue(workflowName, out var continuatorTypes)
               && continuatorTypes.TryGetValue(stageName, out var continuatorType)
               && _serviceProviderIsService.IsService(continuatorType);
    }

    /// <inheritdoc/>
    public bool TryGet(string workflowName, string stageName, [NotNullWhen(true)] out IWorkflowContinuator? workflowContinuator)
    {
        WorkflowException.ThrowIfNullOrWhiteSpace(workflowName);

        if (_continuatorTypes.TryGetValue(workflowName, out var continuatorTypes)
            && continuatorTypes.TryGetValue(stageName, out var continuatorType)
            && _serviceProvider.GetService(continuatorType) is IWorkflowContinuator continuator)
        {
            workflowContinuator = continuator;
            return true;
        }
        workflowContinuator = default;
        return false;
    }

    #endregion Public 方法
}
