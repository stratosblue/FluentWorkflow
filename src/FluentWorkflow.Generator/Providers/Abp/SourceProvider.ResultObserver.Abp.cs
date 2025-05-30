﻿using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class AbpResultObserverSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public AbpResultObserverSourceProvider(GenerateContext generateContext) : base(generateContext)
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

partial class {WorkflowName}ResultObserver : IDistributedEventHandler<{WorkflowName}FinishedMessage>
{{
    /// <summary>
    /// Abp 的 <see cref=""ICancellationTokenProvider""/>
    /// </summary>
    protected ICancellationTokenProvider? CancellationTokenProvider => ServiceProvider.GetService<ICancellationTokenProvider>();

    /// <summary>
    /// 处理消息 - <see cref=""{WorkflowName}FinishedMessage""/>
    /// </summary>
    /// <param name=""eventData""></param>
    /// <returns></returns>
    public virtual Task HandleEventAsync({WorkflowName}FinishedMessage eventData)
    {{
        return HandleAsync(eventData, CancellationTokenProvider?.Token ?? default);
    }}
}}");

        yield return new($"Workflow.{WorkflowName}.ResultObserver.Abp.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
