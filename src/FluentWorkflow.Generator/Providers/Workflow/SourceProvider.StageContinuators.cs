using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StageContinuatorsSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public StageContinuatorsSourceProvider(GenerateContext context) : base(context)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(2048);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}.{WorkflowName}.Continuator;

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程延续器
/// </summary>
/// <typeparam name=""TStageFinalizer"">阶段完成器类型</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowName}Continuator<TStageFinalizer>
    : WorkflowContinuator<TStageFinalizer, I{WorkflowClassName}>
    where TStageFinalizer : I{WorkflowName}StageFinalizer, I{WorkflowClassName}
{{
    /// <summary>
    /// 阶段完成器
    /// </summary>
    protected readonly TStageFinalizer StageFinalizer;

    /// <inheritdoc cref=""{WorkflowName}Continuator{{TStageFinalizer}}""/>
    public {WorkflowName}Continuator(TStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider) : base(workflowAwaitProcessor, logger, serviceProvider)
    {{
        StageFinalizer = stageFinalizer ?? throw new ArgumentNullException(nameof(stageFinalizer));
    }}

    /// <inheritdoc/>
    protected override Task<TStageFinalizer> GetStageFinalizerAsync(IWorkflowFinishedMessage childWorkflowFinishedMessage, WorkflowContextSnapshot parentContextSnapshot, CancellationToken cancellationToken)
    {{
        return Task.FromResult<TStageFinalizer>(StageFinalizer);
    }}
}}
");

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的流程延续器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class Stage{stage.Name}ContinuatorBase
    : {WorkflowName}Continuator<IStage{stage.Name}Finalizer>
    , IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{{
    /// <inheritdoc/>
    public static string WorkflowName => {WorkflowClassName}.WorkflowName;

    /// <inheritdoc/>
    public static string StageName => {WorkflowName}Stages.{stage.Name};

    /// <inheritdoc cref=""Stage{stage.Name}ContinuatorBase""/>
    protected Stage{stage.Name}ContinuatorBase(IStage{stage.Name}Finalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {{
    }}
}}");
        }

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的流程延续器
/// </summary>
public partial class Stage{stage.Name}Continuator : Stage{stage.Name}ContinuatorBase
{{
    /// <inheritdoc cref=""Stage{stage.Name}Continuator""/>
    public Stage{stage.Name}Continuator(IStage{stage.Name}Finalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger<Stage{stage.Name}Continuator> logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {{
    }}
}}");
        }

        yield return new($"Workflow.{WorkflowName}.StageContinuators.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
