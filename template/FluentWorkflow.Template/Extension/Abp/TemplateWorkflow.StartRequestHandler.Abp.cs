﻿// <Auto-Generated/>

using Microsoft.Extensions.DependencyInjection;
using TemplateNamespace.Message;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

namespace TemplateNamespace.Handler;

partial class TemplateWorkflowStartRequestHandler<TWorkflow> : IDistributedEventHandler<TemplateWorkflowStartRequestMessage>
{
    /// <summary>
    /// 处理消息 - <see cref="TemplateWorkflowStartRequestMessage"/>
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns></returns>
    public virtual Task HandleEventAsync(TemplateWorkflowStartRequestMessage eventData)
    {
        return HandleAsync(eventData, ServiceProvider.GetService<ICancellationTokenProvider>()?.Token ?? default);
    }
}
