using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class BuilderSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public BuilderSourceProvider(GenerateContext generateContext) : base(generateContext)
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
/// <see cref=""{WorkflowName}""/> 构造器基类
/// </summary>
/// <typeparam name=""TWorkflow""><see cref=""{WorkflowName}""/> 或其派生类型</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
internal abstract partial class {WorkflowName}BuilderBase<TWorkflow>
    : SimpleWorkflowBuilder<TWorkflow, {WorkflowName}Context, I{WorkflowName}>
    where TWorkflow : {WorkflowName}
{{
    /// <inheritdoc cref=""{WorkflowName}BuilderBase{{TWorkflow}}""/>
    protected {WorkflowName}BuilderBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override {WorkflowName}Context BuildContext(IEnumerable<KeyValuePair<string, string>> context)
    {{
        return new {WorkflowName}Context(context);
    }}
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 构造器
/// </summary>
/// <typeparam name=""TWorkflow""><see cref=""{WorkflowName}""/> 或其派生类型</typeparam>
internal partial class {WorkflowName}Builder<TWorkflow>
    : {WorkflowName}BuilderBase<TWorkflow>
    where TWorkflow : {WorkflowName}
{{
    /// <inheritdoc cref=""{WorkflowName}Builder{{TWorkflow}}""/>
    public {WorkflowName}Builder(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}
}}
");
        yield return new($"{WorkflowName}.Builder.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
