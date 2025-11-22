using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class AbpStageHandlersSourceProvider(GenerateContext generateContext)
    : WorkflowSourceProvider(generateContext)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        #region Handlers

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

namespace {NameSpace}.{WorkflowName}.Handler;
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
partial class Stage{stage.Name}HandlerBase : IDistributedEventHandler<Stage{stage.Name}Message>
{{
    /// <summary>
    /// Abp 的 <see cref=""ICancellationTokenProvider""/>
    /// </summary>
    protected ICancellationTokenProvider? CancellationTokenProvider => ServiceProvider.GetService<ICancellationTokenProvider>();

    /// <summary>
    /// 处理消息 - <see cref=""Stage{stage.Name}Message""/>
    /// </summary>
    /// <param name=""eventData""></param>
    /// <returns></returns>
    public virtual Task HandleEventAsync(Stage{stage.Name}Message eventData)
    {{
        return HandleAsync(eventData, CancellationTokenProvider?.Token ?? default);
    }}
}}
");
        }

        yield return new($"Workflow.{WorkflowName}.StageHandlers.Abp.g.cs", builder.ToString());

        #endregion Handlers
    }

    #endregion Public 方法
}
