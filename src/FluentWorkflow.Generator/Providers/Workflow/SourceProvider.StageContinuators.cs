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

namespace {NameSpace}.Continuator;

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程延续器
/// </summary>
/// <typeparam name=""TStageFinalizer"">阶段完成器类型</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowClassName}Continuator<TStageFinalizer>
    : WorkflowContinuator<TStageFinalizer, I{WorkflowClassName}>
    where TStageFinalizer : I{WorkflowClassName}StageFinalizer, I{WorkflowClassName}
{{
    /// <summary>
    /// 阶段完成器
    /// </summary>
    protected readonly TStageFinalizer StageFinalizer;

    /// <inheritdoc cref=""{WorkflowClassName}Continuator{{TStageFinalizer}}""/>
    public {WorkflowClassName}Continuator(TStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider) : base(workflowAwaitProcessor, logger, serviceProvider)
    {{
        StageFinalizer = stageFinalizer ?? throw new ArgumentNullException(nameof(stageFinalizer));
    }}

    /// <inheritdoc/>
    protected override Task<TStageFinalizer> GetStageFinalizerAsync(IWorkflowFinishedMessage childWorkflowFinishedMessage, WorkflowContextMetadata parentContextMetadata, CancellationToken cancellationToken)
    {{
        return Task.FromResult<TStageFinalizer>(StageFinalizer);
    }}
}}
");

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowClassName}Stages.{stage.Name}""/> 的流程延续器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowClassName}{stage.Name}ContinuatorBase
    : {WorkflowClassName}Continuator<I{WorkflowClassName}{stage.Name}StageFinalizer>
    , IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{{
    /// <inheritdoc/>
    public static string WorkflowName => {WorkflowClassName}.WorkflowName;

    /// <inheritdoc/>
    public static string StageName => {WorkflowClassName}Stages.{stage.Name};

    /// <inheritdoc cref=""{WorkflowClassName}{stage.Name}ContinuatorBase""/>
    protected {WorkflowClassName}{stage.Name}ContinuatorBase(I{WorkflowClassName}{stage.Name}StageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {{
    }}
}}");
        }

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowClassName}Stages.{stage.Name}""/> 的流程延续器
/// </summary>
public partial class {WorkflowClassName}{stage.Name}Continuator : {WorkflowClassName}{stage.Name}ContinuatorBase
{{
    /// <inheritdoc cref=""{WorkflowClassName}{stage.Name}Continuator""/>
    public {WorkflowClassName}{stage.Name}Continuator(I{WorkflowClassName}{stage.Name}StageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger<{WorkflowClassName}{stage.Name}Continuator> logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {{
    }}
}}");
        }

        yield return new($"{WorkflowClassName}.StageContinuators.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
