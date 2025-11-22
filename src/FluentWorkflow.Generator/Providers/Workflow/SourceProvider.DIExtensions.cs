using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class DIExtensionsSourceProvider(GenerateContext generateContext)
    : WorkflowSourceProvider(generateContext)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class {WorkflowClassName}DIExtensions
{{
    /// <summary>
    /// 添加 <see cref=""{WorkflowClassName}""/> 的构造器，以支持使用 <see cref=""IWorkflowBuilder{{TWorkflow}}""/> 构造并发起 <see cref=""{WorkflowClassName}""/>
    /// </summary>
    /// <param name=""builder""></param>
    /// <param name=""setupAction"">配置委托</param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder Add{WorkflowClassName}(this IFluentWorkflowBuilder builder, Action<{WorkflowClassName}Configuration>? setupAction = null) => builder.Add{WorkflowClassName}<{WorkflowClassName}>(setupAction);

    /// <summary>
    /// 添加 <see cref=""{WorkflowClassName}""/> 的构造器，以支持使用 <see cref=""IWorkflowBuilder{{TWorkflow}}""/> 构造并发起 <see cref=""{WorkflowClassName}""/>
    /// <br/>使用 <typeparamref name=""TWorkflow""/> 替代默认调度实现类型 <see cref=""{WorkflowClassName}""/>
    /// </summary>
    /// <typeparam name=""TWorkflow""></typeparam>
    /// <param name=""builder""></param>
    /// <param name=""setupAction"">配置委托</param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder Add{WorkflowClassName}<TWorkflow>(this IFluentWorkflowBuilder builder, Action<{WorkflowClassName}Configuration>? setupAction = null)
        where TWorkflow : {WorkflowClassName}
    {{
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowBuilder<{WorkflowClassName}>), typeof({WorkflowClassName}Builder<TWorkflow>), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowBuilder<TWorkflow>), typeof({WorkflowClassName}Builder<TWorkflow>), ServiceLifetime.Scoped));

        if (setupAction is not null)
        {{
            var workflowConfiguration = new {WorkflowClassName}Configuration<TWorkflow>(builder);
            setupAction(workflowConfiguration);
        }}

        return builder;
    }}

    /// <summary>
    /// 添加 <see cref=""{WorkflowClassName}""/> 的结果观察器 <see cref=""{WorkflowName}ResultObserver""/>
    /// </summary>
    /// <param name=""configuration""></param>
    /// <returns></returns>
    public static {WorkflowClassName}Configuration AddResultObserver(this {WorkflowClassName}Configuration configuration) => configuration.AddResultObserver<{WorkflowName}ResultObserver>();

    /// <summary>
    /// 添加 <see cref=""{WorkflowClassName}""/> 的结果观察器，使用 <typeparamref name=""TWorkflowResultObserver""/> 替代默认的 <see cref=""{WorkflowName}ResultObserver""/>
    /// </summary>
    /// <param name=""configuration""></param>
    /// <returns></returns>
    public static {WorkflowClassName}Configuration AddResultObserver<TWorkflowResultObserver>(this {WorkflowClassName}Configuration configuration)
        where TWorkflowResultObserver : {WorkflowName}ResultObserverBase
    {{
        var builder = configuration.Builder;

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowClassName}, TWorkflowResultObserver, global::{NameSpace}.{WorkflowName}.Message.{WorkflowName}FinishedMessage, I{WorkflowClassName}>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResultObserver<{WorkflowClassName}>), typeof(TWorkflowResultObserver), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TWorkflowResultObserver), typeof(TWorkflowResultObserver), ServiceLifetime.Scoped));
        return configuration;
    }}
}}
");
        yield return new($"Workflow.{WorkflowName}.DIExtensions.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
