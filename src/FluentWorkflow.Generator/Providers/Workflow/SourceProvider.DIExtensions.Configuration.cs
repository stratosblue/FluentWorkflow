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
/// <see cref=""{WorkflowName}""/> 配置
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class {WorkflowName}Configuration : I{WorkflowName}
{{
    /// <inheritdoc cref=""IFluentWorkflowBuilder""/>
    public IFluentWorkflowBuilder Builder {{ get; }}

    /// <inheritdoc cref=""IServiceCollection""/>
    public IServiceCollection Services {{ get; }}

    /// <summary>
    /// 最终使用的流程实现类型
    /// </summary>
    public abstract Type WorkflowType {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}Configuration{{TWorkflow}}""/>
    internal {WorkflowName}Configuration(IFluentWorkflowBuilder builder)
    {{
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        Services = builder.Services ?? throw new ArgumentNullException(nameof(builder.Services));
    }}

    /// <summary>
    /// 添加 <see cref=""{WorkflowName}""/> 的调度器
    /// </summary>
    /// <returns></returns>
    public abstract {WorkflowName}Configuration AddScheduler();
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 配置
/// <br/>使用的流程实现为 <typeparamref name=""TWorkflow""/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class {WorkflowName}Configuration<TWorkflow>
    : {WorkflowName}Configuration
    where TWorkflow : {WorkflowName}
{{
    /// <inheritdoc/>
    public override Type WorkflowType {{ get; }} = typeof(TWorkflow);

    /// <inheritdoc cref=""{WorkflowName}Configuration""/>
    internal {WorkflowName}Configuration(IFluentWorkflowBuilder builder) : base(builder)
    {{
    }}

    /// <summary>
    /// 添加 <see cref=""{WorkflowName}""/> 的调度器
    /// </summary>
    /// <returns></returns>
    public override {WorkflowName}Configuration AddScheduler()
    {{
        var builder = Builder;

        #region StartRequestHandler

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowName}, {WorkflowName}StartRequestHandler<TWorkflow>, {WorkflowName}StartRequestMessage, I{WorkflowName}>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof({WorkflowName}StartRequestHandler<TWorkflow>), typeof({WorkflowName}StartRequestHandler<TWorkflow>), ServiceLifetime.Scoped));

        #endregion StartRequestHandler

        #region StateMachineDriver

");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($"builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowName}, {WorkflowName}StateMachineDriver, {WorkflowName}{stage.Name}StageCompletedMessage, I{WorkflowName}>();");
        }

        builder.AppendLine($@"
        builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowName}, {WorkflowName}StateMachineDriver, {WorkflowName}FailureMessage, I{WorkflowName}>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof({WorkflowName}StateMachineDriver), typeof({WorkflowName}StateMachineDriver), ServiceLifetime.Scoped));

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResumer<{WorkflowName}>), static serviceProvider => serviceProvider.GetRequiredService<{WorkflowName}StateMachineDriver>(), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowResumer<TWorkflow>), static serviceProvider => serviceProvider.GetRequiredService<{WorkflowName}StateMachineDriver>(), ServiceLifetime.Scoped));

        #endregion StateMachineDriver

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowScheduler<{WorkflowName}>), typeof({WorkflowName}Scheduler), ServiceLifetime.Scoped));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowScheduler<TWorkflow>), typeof({WorkflowName}Scheduler), ServiceLifetime.Scoped));

        return this;
    }}
}}
");
        yield return new($"{WorkflowName}.DIExtensions.Configuration.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
