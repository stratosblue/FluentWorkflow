using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class DIExtensionsSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public DIExtensionsSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

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
public static class {WorkflowName}DIExtensions
{{
    /// <summary>
    /// 添加 <see cref=""{WorkflowName}""/> 的构造器，以支持使用 <see cref=""IWorkflowBuilder{{TWorkflow}}""/> 构造并发起 <see cref=""{WorkflowName}""/>
    /// </summary>
    /// <param name=""builder""></param>
    /// <param name=""setupAction"">配置委托</param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder Add{WorkflowName}(this IFluentWorkflowBuilder builder, Action<{WorkflowName}Configuration>? setupAction = null) => builder.Add{WorkflowName}<{WorkflowName}>(setupAction);

    /// <summary>
    /// 添加 <see cref=""{WorkflowName}""/> 的构造器，以支持使用 <see cref=""IWorkflowBuilder{{TWorkflow}}""/> 构造并发起 <see cref=""{WorkflowName}""/>
    /// <br/>使用 <typeparamref name=""TWorkflow""/> 替代默认调度实现类型 <see cref=""{WorkflowName}""/>
    /// </summary>
    /// <typeparam name=""TWorkflow""></typeparam>
    /// <param name=""builder""></param>
    /// <param name=""setupAction"">配置委托</param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder Add{WorkflowName}<TWorkflow>(this IFluentWorkflowBuilder builder, Action<{WorkflowName}Configuration>? setupAction = null)
        where TWorkflow : {WorkflowName}
    {{
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowBuilder<{WorkflowName}>), typeof({WorkflowName}Builder<TWorkflow>), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowBuilder<TWorkflow>), typeof({WorkflowName}Builder<TWorkflow>), ServiceLifetime.Scoped));

        if (setupAction is not null)
        {{
            var workflowConfiguration = new {WorkflowName}Configuration<TWorkflow>(builder);
            setupAction(workflowConfiguration);
        }}

        return builder;
    }}

    /// <summary>
    /// 添加 <see cref=""{WorkflowName}""/> 的结果观察器 <see cref=""{WorkflowName}ResultObserver""/>
    /// </summary>
    /// <param name=""configuration""></param>
    /// <returns></returns>
    public static {WorkflowName}Configuration AddResultObserver(this {WorkflowName}Configuration configuration) => configuration.AddResultObserver<{WorkflowName}ResultObserver>();

    /// <summary>
    /// 添加 <see cref=""{WorkflowName}""/> 的结果观察器，使用 <typeparamref name=""TWorkflowResultObserver""/> 替代默认的 <see cref=""{WorkflowName}ResultObserver""/>
    /// </summary>
    /// <param name=""configuration""></param>
    /// <returns></returns>
    public static {WorkflowName}Configuration AddResultObserver<TWorkflowResultObserver>(this {WorkflowName}Configuration configuration)
        where TWorkflowResultObserver : {WorkflowName}ResultObserverBase
    {{
        var builder = configuration.Builder;

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowName}, TWorkflowResultObserver, {WorkflowName}FinishedMessage, I{WorkflowName}>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResultObserver<{WorkflowName}>), typeof(TWorkflowResultObserver), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TWorkflowResultObserver), typeof(TWorkflowResultObserver), ServiceLifetime.Scoped));
        return configuration;
    }}
}}
");
        yield return new($"{WorkflowName}.DIExtensions.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
