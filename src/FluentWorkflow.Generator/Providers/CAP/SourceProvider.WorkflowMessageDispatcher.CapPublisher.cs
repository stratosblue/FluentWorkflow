using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class CapPublisherWorkflowMessageDispatcherSourceProvider : SourceProvider
{
    #region Private 字段

    private readonly string _nameSpacePostFix;

    #endregion Private 字段

    #region Public 构造函数

    public CapPublisherWorkflowMessageDispatcherSourceProvider(string nameSpacePostFix)
    {
        _nameSpacePostFix = nameSpacePostFix;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentWorkflow.GenericExtension{_nameSpacePostFix};

/// <summary>
/// 基于 cap 的 <inheritdoc cref=""IWorkflowMessageDispatcher""/> 默认实现
/// </summary>
internal partial class CapPublisherWorkflowMessageDispatcher : WorkflowMessageDispatcher
{{
    private readonly ICapPublisher _capPublisher;

    /// <inheritdoc cref=""CapPublisherWorkflowMessageDispatcher""/>
    public CapPublisherWorkflowMessageDispatcher(ICapPublisher capPublisher, IWorkflowDiagnosticSource diagnosticSource, ILoggerFactory loggerFactory)
        : base(diagnosticSource, loggerFactory?.CreateLogger(""FluentWorkflow.CapPublisherWorkflowMessageDispatcher""))
    {{
        _capPublisher = capPublisher ?? throw new ArgumentNullException(nameof(capPublisher));
    }}

    /// <inheritdoc/>
    public override async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {{
        await base.PublishAsync(message, cancellationToken);
        await _capPublisher.PublishAsync(TMessage.EventName, message, cancellationToken: cancellationToken);
    }}
}}

/// <summary>
/// 
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class CapPublisherWorkflowMessageDispatcherDIExtensions
{{
    /// <summary>
    /// 使用基于 cap 的 <inheritdoc cref=""IWorkflowMessageDispatcher""/>
    /// </summary>
    /// <param name=""builder""></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder UseCapPublisherWorkflowMessageDispatcher(this IFluentWorkflowBuilder builder)
    {{
        builder.Services.Replace(ServiceDescriptor.Scoped<IWorkflowMessageDispatcher, CapPublisherWorkflowMessageDispatcher>());
        return builder;
    }}
}}
");

        yield return new("WorkflowMessageDispatcher.CapPublisher.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
