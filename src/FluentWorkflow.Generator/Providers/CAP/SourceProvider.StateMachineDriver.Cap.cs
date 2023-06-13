using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class CapStateMachineDriverSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public CapStateMachineDriverSourceProvider(GenerateContext generateContext) : base(generateContext)
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

namespace {NameSpace}.Internal;

partial class {WorkflowName}StateMachineDriverBase : ICapSubscribe
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
    /// 处理消息 <inheritdoc cref=""{WorkflowName}{stage.Name}StageCompletedMessage.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe({WorkflowName}{stage.Name}StageCompletedMessage.EventName)]
    public virtual Task HandleMessageAsync({WorkflowName}{stage.Name}StageCompletedMessage message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
");
        }

        builder.Append("}");

        yield return new($"{WorkflowName}.StateMachineDriver.Cap.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
