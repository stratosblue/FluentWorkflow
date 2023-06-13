using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StageBuilderSourceProvider : WorkflowSourceProvider
{
    public StageBuilderSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace};

/// <summary>
/// <see cref=""{WorkflowName}""/> 阶段构造器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface I{WorkflowName}StageBuilder
{{
    /// <summary>
    /// 声明流程开始
    /// </summary>
    /// <returns></returns>
    public I{WorkflowName}flowStageBuilder Begin();
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 流程阶段构造器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface I{WorkflowName}flowStageBuilder
{{
    /// <summary>
    /// 声明阶段
    /// </summary>
    /// <param name=""stageName"">阶段名称（需要为有效的 C# 类型名称字符）</param>
    /// <returns></returns>
    public I{WorkflowName}flowStageBuilder Then(string stageName);

    /// <summary>
    /// 声明阶段全部结束
    /// </summary>
    public void Completion();
}}
");

        yield return new($"{WorkflowName}.StageBuilders.g.cs", builder.ToString());
    }
}
