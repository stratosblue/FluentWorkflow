using System.Text;
using FluentWorkflow.Generator.Model;
using Microsoft.CodeAnalysis;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class ContextSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public ContextSourceProvider(GenerateContext context) : base(context)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(2048);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using System.Text.Json.Serialization;

namespace {NameSpace};

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的上下文基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowContextName}Base
    : WorkflowContext, I{WorkflowClassName}
{{
");
        foreach (var contextProperty in Context.WorkflowDeclaration.ContextProperties)
        {
            var nullable = contextProperty.Type.IsValueType ? "" : "?";
            builder.AppendLine($$"""
                                    /// <summary>
                                    /// {{contextProperty.Comment}}
                                    /// </summary>
                                    public virtual {{contextProperty.Type.ToFullCodeString()}}{{nullable}} {{contextProperty.Name}} { get => InnerGet<{{contextProperty.Type.ToFullCodeString()}}>(); set => InnerSet(value); }
                                """);
        }
        builder.AppendLine($@"
    /// <inheritdoc/>
    protected sealed override string WorkflowName => {WorkflowClassName}.WorkflowName;

    /// <inheritdoc cref=""{WorkflowContextName}Base""/>
    protected {WorkflowContextName}Base(string id) : base(id)
    {{
    }}

    /// <inheritdoc cref=""{WorkflowContextName}Base""/>
    protected {WorkflowContextName}Base(IEnumerable<KeyValuePair<string, string>> values) : base(values)
    {{
    }}

    /// <inheritdoc/>
    protected override string CheckBeforeSetCurrentStage(string stage)
    {{
        if ({Names.WorkflowNameStagesClass}.StageIds.Contains(stage))
        {{
            return stage;
        }}
        throw new WorkflowInvalidOperationException($""未知的阶段：{{stage}}"");
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的上下文
/// </summary>
[JsonConverter(typeof(KeyValuesConvertableJsonConverter<{WorkflowContextName}>))]
public sealed partial class {WorkflowContextName}
    : {WorkflowContextName}Base
    , IKeyValuesConvertable<{WorkflowContextName}>
{{
    /// <inheritdoc cref=""{WorkflowContextName}""/>
    public {WorkflowContextName}() : this(Guid.NewGuid().ToString())
    {{
    }}

    /// <inheritdoc cref=""{WorkflowContextName}""/>
    public {WorkflowContextName}(string id) : base(id)
    {{
    }}

    /// <inheritdoc cref=""{WorkflowContextName}""/>
    public {WorkflowContextName}(IEnumerable<KeyValuePair<string, string>> values) : base(values)
    {{
    }}

    /// <inheritdoc/>
    public static {WorkflowContextName} ConstructFromKeyValues(IEnumerable<KeyValuePair<string, string>> values)
    {{
        return new(values);
    }}

    /// <inheritdoc/>
    public static IEnumerable<KeyValuePair<string, string>> ConvertToKeyValues({WorkflowContextName} instance)
    {{
        return instance.GetSnapshot();
    }}
}}
");
        yield return new($"Workflow.{WorkflowName}.Context.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
