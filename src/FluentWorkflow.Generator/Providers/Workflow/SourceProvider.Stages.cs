using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StagesSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public StagesSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace};

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段定义
/// </summary>
public static class {WorkflowClassName}Stages
{{
    /// <summary>
    /// 有序的阶段名称列表
    /// </summary>
    public static ImmutableArray<string> OrderedStages {{ get; }}

    /// <summary>
    /// 有序的阶段ID列表
    /// </summary>
    public static ImmutableArray<string> OrderedStageIds {{ get; }}

    /// <summary>
    /// 去重的阶段列表
    /// </summary>
    public static ImmutableHashSet<string> Stages {{ get; }}

    /// <summary>
    /// 去重的阶段ID列表
    /// </summary>
    public static ImmutableHashSet<string> StageIds {{ get; }}
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
    /// <summary>
    /// 阶段 {stage.Name}
    /// </summary>
    public const string {stage.Name} = ""{WorkflowClassName}.Stage.{stage.Name}"";
");
        }

        builder.AppendLine($@"/// <summary>
    /// 阶段 Completion
    /// </summary>
    public const string Completion = ""{WorkflowClassName}.Stage.Completion"";

    /// <summary>
    /// 阶段 Failure
    /// </summary>
    public const string Failure = ""{WorkflowClassName}.Stage.Failure"";
");

        builder.AppendLine($@"
    /// <inheritdoc cref=""{WorkflowClassName}Stages""/>
    static {WorkflowClassName}Stages()
    {{");
        builder.AppendLine($"OrderedStageIds = ImmutableArray.Create({string.Join(", ", Context.Stages.Select(m => m.Name))});");
        builder.AppendLine($"OrderedStages = ImmutableArray.Create({string.Join(", ", Context.Stages.Select(m => $"nameof({m.Name})"))});");

        builder.AppendLine($"StageIds = ImmutableHashSet.Create({string.Join(", ", Context.Stages.Select(m => m.Name))}, {Names.WorkflowCompletionStageConstantName}, {Names.WorkflowFailureStageConstantName});");
        builder.AppendLine($"Stages = ImmutableHashSet.Create({string.Join(", ", Context.Stages.Select(m => $"nameof({m.Name})"))}, nameof({Names.WorkflowCompletionStageConstantName}), nameof({Names.WorkflowFailureStageConstantName}));");

        builder.AppendLine($@"}}
}}

/// <summary>
/// 标记接口 - 标记属于工作流程 <see cref=""{WorkflowClassName}""/>
/// </summary>
public interface I{WorkflowClassName} {{ }}

");

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"/// <summary>
/// 标记接口 - 标记属于工作流程 <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowClassName}Stages.{stage.Name}""/>
/// </summary>
public interface I{WorkflowClassName}{stage.Name}Stage : IWorkflowStage, I{WorkflowClassName} {{ }}");
        }

        yield return new($"{WorkflowClassName}.Stages.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
