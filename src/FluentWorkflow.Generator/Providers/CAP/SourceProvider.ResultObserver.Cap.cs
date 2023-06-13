using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class CapResultObserverSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public CapResultObserverSourceProvider(GenerateContext generateContext) : base(generateContext)
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

partial class {WorkflowName}ResultObserver : ICapSubscribe
{{
    /// <summary>
    /// 处理消息 <inheritdoc cref=""{WorkflowName}FinishedMessage.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe({WorkflowName}FinishedMessage.EventName)]
    public virtual Task HandleMessageAsync({WorkflowName}FinishedMessage message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
}}
");

        yield return new($"{WorkflowName}.ResultObserver.Cap.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
