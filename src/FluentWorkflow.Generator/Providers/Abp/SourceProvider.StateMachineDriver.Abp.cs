using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class AbpStateMachineDriverSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public AbpStateMachineDriverSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

namespace {NameSpace}.Internal;

partial class {WorkflowName}StateMachineDriverBase
   : IDistributedEventHandler<{WorkflowName}FailureMessage>
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"   , IDistributedEventHandler<{WorkflowName}{stage.Name}StageCompletedMessage>");
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
    /// 处理消息 - <see cref=""{WorkflowName}{stage.Name}StageCompletedMessage""/>
    /// </summary>
    /// <param name=""eventData""></param>
    /// <returns></returns>
    public virtual Task HandleEventAsync({WorkflowName}{stage.Name}StageCompletedMessage eventData)
    {{
        var cancellationToken = CancellationTokenProvider.Token;
        return HandleAsync(eventData, cancellationToken);
    }}
");
        }

        builder.Append("}");

        yield return new($"{WorkflowName}.StateMachineDriver.Abp.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
