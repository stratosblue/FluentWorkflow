using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class DIExtensionsStageHandlerSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public DIExtensionsStageHandlerSourceProvider(GenerateContext generateContext) : base(generateContext)
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
public static class {WorkflowName}StageHandlerDIExtensions
{{
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
    /// <summary>
    /// 添加对工作流程 <see cref=""{WorkflowName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的处理器
    /// </summary>
    /// <typeparam name=""THandler"">基于 <see cref=""{WorkflowName}{stage.Name}StageHandlerBase""/> 实现的处理器类型</typeparam>
    /// <param name=""builder""></param>
    /// <param name=""serviceLifetime""></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder Add{WorkflowName}{stage.Name}StageHandler<THandler>(this IFluentWorkflowBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where THandler : {WorkflowName}{stage.Name}StageHandlerBase
    {{
        return builder.Add{WorkflowName}{stage.Name}StageHandler<THandler, {WorkflowName}{stage.Name}Continuator>(serviceLifetime);
    }}

    /// <summary>
    /// 添加对工作流程 <see cref=""{WorkflowName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的处理器
    /// </summary>
    /// <typeparam name=""THandler"">基于 <see cref=""{WorkflowName}{stage.Name}StageHandlerBase""/> 实现的处理器类型</typeparam>
    /// <typeparam name=""TContinuator"">基于 <see cref=""{WorkflowName}{stage.Name}ContinuatorBase""/> 实现的流程延续器类型</typeparam>
    /// <param name=""builder""></param>
    /// <param name=""serviceLifetime""></param>
    /// <returns></returns>
    public static IFluentWorkflowBuilder Add{WorkflowName}{stage.Name}StageHandler<THandler, TContinuator>(this IFluentWorkflowBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where THandler : {WorkflowName}{stage.Name}StageHandlerBase
        where TContinuator : {WorkflowName}{stage.Name}ContinuatorBase
    {{
        ServiceCollectionUniqueAddHelper.RegisterContinuator<TContinuator>(builder.Services, serviceLifetime);

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowName}, THandler, {WorkflowName}{stage.Name}StageMessage, I{WorkflowName}>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(THandler), typeof(THandler), serviceLifetime));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(I{WorkflowName}{stage.Name}StageFinalizer), typeof(THandler), serviceLifetime));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowStageHandler<{WorkflowName}{stage.Name}StageMessage>), typeof(THandler), serviceLifetime));

        return builder;
    }}
");
        }

        builder.AppendLine("}");
        yield return new($"{WorkflowName}.DIExtensions.StageHandler.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
