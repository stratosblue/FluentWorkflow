using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class AbpDistributedEventBusWorkflowMessageDispatcherSourceProvider : SourceProvider
{
    #region Private 字段

    private readonly string _nameSpacePostFix;

    #endregion Private 字段

    #region Public 构造函数

    public AbpDistributedEventBusWorkflowMessageDispatcherSourceProvider(string nameSpacePostFix)
    {
        _nameSpacePostFix = nameSpacePostFix;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.EventBus.Distributed;

namespace FluentWorkflow.GenericExtension{_nameSpacePostFix};

/// <summary>
/// 基于 abp 的 <inheritdoc cref=""IWorkflowMessageDispatcher""/> 默认实现
/// </summary>
internal partial class AbpDistributedWorkflowMessageDispatcher : WorkflowMessageDispatcher
{{
    private readonly IDistributedEventBus _distributedEventBus;

    /// <inheritdoc cref=""AbpDistributedWorkflowMessageDispatcher""/>
    public AbpDistributedWorkflowMessageDispatcher(IDistributedEventBus distributedEventBus, IWorkflowDiagnosticSource diagnosticSource, ILoggerFactory loggerFactory)
        : base(diagnosticSource, loggerFactory?.CreateLogger(""FluentWorkflow.AbpDistributedWorkflowMessageDispatcher""))
    {{
        _distributedEventBus = distributedEventBus ?? throw new ArgumentNullException(nameof(distributedEventBus));
    }}

    /// <inheritdoc/>
    public override async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {{
        await base.PublishAsync(message, cancellationToken);
        await _distributedEventBus.PublishAsync(message, true);
    }}
}}

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class AbpDistributedWorkflowMessageDispatcherDIExtensions
{{
    /// <summary>
    /// 使用基于 abp 的 <inheritdoc cref=""IWorkflowMessageDispatcher""/>
    /// </summary>
    /// <param name=""builder""></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseAbpDistributedWorkflowMessageDispatcher(this IFluentWorkflowBuilder builder)
    {{
        builder.Services.Replace(ServiceDescriptor.Scoped<IWorkflowMessageDispatcher, AbpDistributedWorkflowMessageDispatcher>());
        return builder;
    }}
}}
");

        yield return new("WorkflowMessageDispatcher.AbpDistributedEventBus.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
