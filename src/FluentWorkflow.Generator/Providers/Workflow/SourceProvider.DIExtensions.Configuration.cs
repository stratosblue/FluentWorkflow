using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class DIExtensionsConfigurationSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public DIExtensionsConfigurationSourceProvider(GenerateContext generateContext) : base(generateContext)
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
/// <see cref=""{WorkflowClassName}""/> 配置
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class {WorkflowClassName}Configuration : I{WorkflowClassName}
{{
    /// <inheritdoc cref=""IFluentWorkflowBuilder""/>
    public IFluentWorkflowBuilder Builder {{ get; }}

    /// <inheritdoc cref=""IServiceCollection""/>
    public IServiceCollection Services {{ get; }}

    /// <summary>
    /// 最终使用的流程实现类型
    /// </summary>
    public abstract Type WorkflowType {{ get; }}

    /// <inheritdoc cref=""{WorkflowClassName}Configuration{{TWorkflow}}""/>
    internal {WorkflowClassName}Configuration(IFluentWorkflowBuilder builder)
    {{
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        Services = builder.Services ?? throw new ArgumentNullException(nameof(builder.Services));
    }}

    /// <summary>
    /// 添加 <see cref=""{WorkflowClassName}""/> 的调度器
    /// </summary>
    /// <returns></returns>
    public abstract {WorkflowClassName}Configuration AddScheduler();
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 配置
/// <br/>使用的流程实现为 <typeparamref name=""TWorkflow""/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class {WorkflowClassName}Configuration<TWorkflow>
    : {WorkflowClassName}Configuration
    where TWorkflow : {WorkflowClassName}
{{
    /// <inheritdoc/>
    public override Type WorkflowType {{ get; }} = typeof(TWorkflow);

    /// <inheritdoc cref=""{WorkflowClassName}Configuration""/>
    internal {WorkflowClassName}Configuration(IFluentWorkflowBuilder builder) : base(builder)
    {{
    }}

    /// <summary>
    /// 添加 <see cref=""{WorkflowClassName}""/> 的调度器
    /// </summary>
    /// <returns></returns>
    public override {WorkflowClassName}Configuration AddScheduler()
    {{
        var builder = Builder;

        #region StartRequestHandler

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowClassName}, {WorkflowClassName}StartRequestHandler<TWorkflow>, {WorkflowClassName}StartRequestMessage, I{WorkflowClassName}>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof({WorkflowClassName}StartRequestHandler<TWorkflow>), typeof({WorkflowClassName}StartRequestHandler<TWorkflow>), ServiceLifetime.Scoped));

        #endregion StartRequestHandler

        #region StateMachineDriver

");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($"builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowClassName}, {WorkflowClassName}StateMachineDriver, {WorkflowClassName}{stage.Name}StageCompletedMessage, I{WorkflowClassName}>();");
        }

        builder.AppendLine($@"
        builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowClassName}, {WorkflowClassName}StateMachineDriver, {WorkflowClassName}FailureMessage, I{WorkflowClassName}>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof({WorkflowClassName}StateMachineDriver), typeof({WorkflowClassName}StateMachineDriver), ServiceLifetime.Scoped));

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResumer<{WorkflowClassName}>), static serviceProvider => serviceProvider.GetRequiredService<{WorkflowClassName}StateMachineDriver>(), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResumer<TWorkflow>), static serviceProvider => serviceProvider.GetRequiredService<{WorkflowClassName}StateMachineDriver>(), ServiceLifetime.Scoped));

        #endregion StateMachineDriver

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowScheduler<{WorkflowClassName}>), typeof({WorkflowClassName}Scheduler), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowScheduler<TWorkflow>), typeof({WorkflowClassName}Scheduler), ServiceLifetime.Scoped));

        return this;
    }}
}}
");
        yield return new($"{WorkflowClassName}.DIExtensions.Configuration.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
