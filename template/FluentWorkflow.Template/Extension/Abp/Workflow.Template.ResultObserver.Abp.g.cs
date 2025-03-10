﻿// <Auto-Generated/>
using Microsoft.Extensions.DependencyInjection;
using TemplateNamespace.Template.Message;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

namespace TemplateNamespace.Template.Handler;

partial class TemplateResultObserver : IDistributedEventHandler<TemplateFinishedMessage>
{
    /// <summary>
    /// Abp 的 <see cref="ICancellationTokenProvider"/>
    /// </summary>
    protected ICancellationTokenProvider? CancellationTokenProvider => ServiceProvider.GetService<ICancellationTokenProvider>();

    /// <summary>
    /// 处理消息 - <see cref="TemplateFinishedMessage"/>
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns></returns>
    public virtual Task HandleEventAsync(TemplateFinishedMessage eventData)
    {
        return HandleAsync(eventData, CancellationTokenProvider?.Token ?? default);
    }
}
