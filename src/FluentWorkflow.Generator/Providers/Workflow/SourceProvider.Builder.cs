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
/// <see cref=""{WorkflowClassName}""/> 构造器基类
/// </summary>
/// <typeparam name=""TWorkflow""><see cref=""{WorkflowClassName}""/> 或其派生类型</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
internal abstract partial class {WorkflowClassName}BuilderBase<TWorkflow>
    : SimpleWorkflowBuilder<TWorkflow, {WorkflowClassName}Context, I{WorkflowClassName}>
    where TWorkflow : {WorkflowClassName}
{{
    /// <inheritdoc cref=""{WorkflowClassName}BuilderBase{{TWorkflow}}""/>
    protected {WorkflowClassName}BuilderBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override {WorkflowClassName}Context BuildContext(IEnumerable<KeyValuePair<string, string>> context)
    {{
        return new {WorkflowClassName}Context(context);
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 构造器
/// </summary>
/// <typeparam name=""TWorkflow""><see cref=""{WorkflowClassName}""/> 或其派生类型</typeparam>
internal partial class {WorkflowClassName}Builder<TWorkflow>
    : {WorkflowClassName}BuilderBase<TWorkflow>
    where TWorkflow : {WorkflowClassName}
{{
    /// <inheritdoc cref=""{WorkflowClassName}Builder{{TWorkflow}}""/>
    public {WorkflowClassName}Builder(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}
}}
");
        yield return new($"{WorkflowClassName}.Builder.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
