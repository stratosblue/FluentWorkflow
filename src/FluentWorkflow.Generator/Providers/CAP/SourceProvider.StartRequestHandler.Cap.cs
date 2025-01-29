using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class CapStartRequestHandlerSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public CapStartRequestHandlerSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using DotNetCore.CAP;

namespace {NameSpace}.{WorkflowName}.Handler;

partial class {WorkflowName}StartRequestHandler<TWorkflow> : ICapSubscribe
{{
    /// <summary>
    /// 处理消息 <inheritdoc cref=""{WorkflowName}StartRequestMessage.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe({WorkflowName}StartRequestMessage.EventName)]
    public Task HandleMessageAsync({WorkflowName}StartRequestMessage message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
}}
");
        yield return new($"Workflow.{WorkflowName}.StartRequestHandler.Cap.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
