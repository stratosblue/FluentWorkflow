using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class AbpStartRequestHandlerSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public AbpStartRequestHandlerSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

namespace {NameSpace}.{WorkflowName}.Handler;

partial class {WorkflowName}StartRequestHandler<TWorkflow> : IDistributedEventHandler<{WorkflowName}StartRequestMessage>
{{
    /// <summary>
    /// 处理消息 - <see cref=""{WorkflowName}StartRequestMessage""/>
    /// </summary>
    /// <param name=""eventData""></param>
    /// <returns></returns>
    public Task HandleEventAsync({WorkflowName}StartRequestMessage eventData)
    {{
        return HandleAsync(eventData, ServiceProvider.GetService<ICancellationTokenProvider>()?.Token ?? default);
    }}
}}
");
        yield return new($"Workflow.{WorkflowName}.StartRequestHandler.Abp.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
