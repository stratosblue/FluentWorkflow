﻿// <Auto-Generated/>

using DotNetCore.CAP;
using TemplateNamespace.Template.Message;

namespace TemplateNamespace.Template.Handler;

partial class TemplateWorkflowStartRequestHandler<TWorkflow> : ICapSubscribe
{
    /// <summary>
    /// 处理消息 <inheritdoc cref="TemplateWorkflowStartRequestMessage.EventName"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [CapSubscribe(TemplateWorkflowStartRequestMessage.EventName)]
    public virtual Task HandleMessageAsync(TemplateWorkflowStartRequestMessage message, CancellationToken cancellationToken)
    {
        return HandleAsync(message, cancellationToken);
    }
}
