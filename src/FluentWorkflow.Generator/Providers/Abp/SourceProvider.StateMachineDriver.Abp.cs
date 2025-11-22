using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class AbpStateMachineDriverSourceProvider(GenerateContext generateContext)
    : WorkflowSourceProvider(generateContext)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

namespace {NameSpace}.{WorkflowName}.Internal;

partial class {WorkflowClassName}StateMachineDriverBase
   : IDistributedEventHandler<{WorkflowName}FailureMessage>
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"   , IDistributedEventHandler<Stage{stage.Name}CompletedMessage>");
        }

        builder.AppendLine($@"
{{
    private ICancellationTokenProvider? _cancellationTokenProvider;
    private ICancellationTokenProvider CancellationTokenProvider => _cancellationTokenProvider ??= ServiceProvider.GetRequiredService<ICancellationTokenProvider>();
");

        builder.AppendLine($@"
    /// <summary>
    /// 处理消息 - <see cref=""{WorkflowName}FailureMessage""/>
    /// </summary>
    /// <param name=""eventData""></param>
    /// <returns></returns>
    public virtual Task HandleEventAsync({WorkflowName}FailureMessage eventData)
    {{
        var cancellationToken = CancellationTokenProvider.Token;
        return HandleAsync(eventData, cancellationToken);
    }}
");

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
    /// <summary>
    /// 处理消息 - <see cref=""Stage{stage.Name}CompletedMessage""/>
    /// </summary>
    /// <param name=""eventData""></param>
    /// <returns></returns>
    public virtual Task HandleEventAsync(Stage{stage.Name}CompletedMessage eventData)
    {{
        var cancellationToken = CancellationTokenProvider.Token;
        return HandleAsync(eventData, cancellationToken);
    }}
");
        }

        builder.Append("}");

        yield return new($"Workflow.{WorkflowName}.StateMachineDriver.Abp.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
