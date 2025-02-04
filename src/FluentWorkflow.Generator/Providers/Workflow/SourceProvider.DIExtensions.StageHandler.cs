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
    /// 添加对工作流程 <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的处理器
    /// </summary>
    /// <typeparam name=""THandler"">基于 <see cref=""Stage{stage.Name}HandlerBase""/> 实现的处理器类型</typeparam>
    /// <param name=""configuration""></param>
    /// <param name=""serviceLifetime""></param>
    /// <returns></returns>
    public static {WorkflowClassName}Configuration AddStage{stage.Name}Handler<THandler>(this {WorkflowClassName}Configuration configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where THandler : Stage{stage.Name}HandlerBase
    {{
        return configuration.AddStage{stage.Name}Handler<THandler, Stage{stage.Name}Continuator>(serviceLifetime);
    }}

    /// <summary>
    /// 添加对工作流程 <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的处理器
    /// </summary>
    /// <typeparam name=""THandler"">基于 <see cref=""Stage{stage.Name}HandlerBase""/> 实现的处理器类型</typeparam>
    /// <typeparam name=""TContinuator"">基于 <see cref=""Stage{stage.Name}ContinuatorBase""/> 实现的流程延续器类型</typeparam>
    /// <param name=""configuration""></param>
    /// <param name=""serviceLifetime""></param>
    /// <returns></returns>
    public static {WorkflowClassName}Configuration AddStage{stage.Name}Handler<THandler, TContinuator>(this {WorkflowClassName}Configuration configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where THandler : Stage{stage.Name}HandlerBase
        where TContinuator : Stage{stage.Name}ContinuatorBase
    {{
        var builder = configuration.Builder;

        ServiceCollectionUniqueAddHelper.RegisterContinuator<TContinuator>(builder.Services, serviceLifetime);

        builder.WorkflowBuildStates.AddEventInvokerDescriptor<{WorkflowClassName}, THandler, Stage{stage.Name}Message, I{WorkflowClassName}>();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(THandler), typeof(THandler), serviceLifetime));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IStage{stage.Name}Finalizer), typeof(THandler), serviceLifetime));
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IWorkflowStageHandler<Stage{stage.Name}Message>), typeof(THandler), serviceLifetime));

        return configuration;
    }}
");
        }

        builder.AppendLine("}");
        yield return new($"Workflow.{WorkflowName}.DIExtensions.StageHandler.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
