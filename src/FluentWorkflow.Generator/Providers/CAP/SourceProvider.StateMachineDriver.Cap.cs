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

partial class {WorkflowClassName}StateMachineDriverBase : ICapSubscribe
{{
    /// <summary>
    /// 处理消息 <inheritdoc cref=""{WorkflowClassName}FailureMessage.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe({WorkflowClassName}FailureMessage.EventName)]
    public virtual Task HandleMessageAsync({WorkflowClassName}FailureMessage message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
    /// <summary>
    /// 处理消息 <inheritdoc cref=""{WorkflowClassName}{stage.Name}StageCompletedMessage.EventName""/>
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    [CapSubscribe({WorkflowClassName}{stage.Name}StageCompletedMessage.EventName)]
    public virtual Task HandleMessageAsync({WorkflowClassName}{stage.Name}StageCompletedMessage message, CancellationToken cancellationToken)
    {{
        return HandleAsync(message, cancellationToken);
    }}
");
        }

        builder.Append("}");

        yield return new($"{WorkflowClassName}.StateMachineDriver.Cap.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
