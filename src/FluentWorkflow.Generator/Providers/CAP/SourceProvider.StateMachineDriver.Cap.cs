using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class CapStateMachineDriverSourceProvider(GenerateContext generateContext)
    : WorkflowSourceProvider(generateContext)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using DotNetCore.CAP;

namespace {NameSpace}.{WorkflowName}.Internal;

partial class {WorkflowClassName}StateMachineDriverBase : ICapSubscribe
{{
    /// <summary>
    /// 处理消息 <inheritdoc cref=""{WorkflowName}FailureMessage.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe({WorkflowName}FailureMessage.EventName)]
    public virtual Task HandleMessageAsync({WorkflowName}FailureMessage message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
    /// <summary>
    /// 处理消息 <inheritdoc cref=""Stage{stage.Name}CompletedMessage.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe(Stage{stage.Name}CompletedMessage.EventName)]
    public virtual Task HandleMessageAsync(Stage{stage.Name}CompletedMessage message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
");
        }

        builder.Append("}");

        yield return new($"Workflow.{WorkflowName}.StateMachineDriver.Cap.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
