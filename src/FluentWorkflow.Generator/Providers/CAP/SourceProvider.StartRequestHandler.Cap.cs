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

namespace {NameSpace}.Handler;

partial class {WorkflowClassName}StartRequestHandler<TWorkflow> : ICapSubscribe
{{
    /// <summary>
    /// 处理消息 <inheritdoc cref=""{WorkflowClassName}StartRequestMessage.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe({WorkflowClassName}StartRequestMessage.EventName)]
    public Task HandleMessageAsync({WorkflowClassName}StartRequestMessage message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
}}
");
        yield return new($"{WorkflowClassName}.StartRequestHandler.Cap.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
