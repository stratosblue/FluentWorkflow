using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class DIExtensionsSchedulerSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public DIExtensionsSchedulerSourceProvider(GenerateContext generateContext) : base(generateContext)
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
public static class {WorkflowName}SchedulerDIExtensions
{{
    /// <summary>
    /// 添加 <see cref=""{WorkflowName}""/> 的调度器
    /// </summary>
    /// <param name=""builder""></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder Add{WorkflowName}Scheduler(this IFluentWorkflowBuilder builder) => builder.Add{WorkflowName}Scheduler<{WorkflowName}>();

    /// <summary>
    /// 添加 <see cref=""{WorkflowName}""/> 的调度器，使用 <typeparamref name=""TWorkflow""/> 替代 <see cref=""{WorkflowName}""/>
    /// </summary>
    /// <typeparam name=""TWorkflow""></typeparam>
    /// <param name=""builder""></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder Add{WorkflowName}Scheduler<TWorkflow>(this IFluentWorkflowBuilder builder) where TWorkflow : {WorkflowName}
    {{
        builder.Add{WorkflowName}<TWorkflow>();

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

        return builder;
    }}
}}
");
        yield return new($"{WorkflowName}.DIExtensions.Scheduler.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
