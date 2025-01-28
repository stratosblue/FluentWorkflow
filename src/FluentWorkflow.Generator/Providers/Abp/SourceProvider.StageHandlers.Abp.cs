using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class AbpStageHandlersSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public AbpStageHandlersSourceProvider(GenerateContext generateContext) : base(generateContext)
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
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

namespace {NameSpace}.Handler;
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
partial class {WorkflowClassName}{stage.Name}StageHandlerBase : IDistributedEventHandler<{WorkflowClassName}{stage.Name}StageMessage>
{{
    /// <summary>
    /// Abp 的 <see cref=""ICancellationTokenProvider""/>
    /// </summary>
    protected ICancellationTokenProvider? CancellationTokenProvider => ServiceProvider.GetService<ICancellationTokenProvider>();

    /// <summary>
    /// 处理消息 - <see cref=""{WorkflowClassName}{stage.Name}StageMessage""/>
    /// </summary>
    /// <param name=""eventData""></param>
    /// <returns></returns>
    public virtual Task HandleEventAsync({WorkflowClassName}{stage.Name}StageMessage eventData)
    {{
        return HandleAsync(eventData, CancellationTokenProvider?.Token ?? default);
    }}
}}
");
        }

        yield return new($"{WorkflowClassName}.StageHandlers.Abp.g.cs", builder.ToString());

        #endregion Handlers
    }

    #endregion Public 方法
}
