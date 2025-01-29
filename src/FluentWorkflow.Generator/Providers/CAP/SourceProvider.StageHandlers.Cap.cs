using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class CapStageHandlersSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public CapStageHandlersSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        #region Handlers

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using DotNetCore.CAP;

namespace {NameSpace}.{WorkflowName}.Handler;
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
partial class Stage{stage.Name}HandlerBase : ICapSubscribe
{{
    /// <summary>
    /// 处理消息 <inheritdoc cref=""Stage{stage.Name}Message.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe(Stage{stage.Name}Message.EventName)]
    public virtual Task HandleMessageAsync(Stage{stage.Name}Message message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
}}
");
        }

        yield return new($"Workflow.{WorkflowName}.StageHandlers.Cap.g.cs", builder.ToString());

        #endregion Handlers
    }

    #endregion Public 方法
}
