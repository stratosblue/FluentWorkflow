﻿// <Auto-Generated/>
using DotNetCore.CAP;
using TemplateNamespace.Template.Message;

namespace TemplateNamespace.Template.Handler;

partial class TemplateWorkflowStage1CAUKStageHandlerBase : ICapSubscribe
{
    /// <summary>
    /// 处理消息 <inheritdoc cref="TemplateWorkflowStage1CAUKStageMessage.EventName"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [CapSubscribe(TemplateWorkflowStage1CAUKStageMessage.EventName)]
    public virtual Task HandleMessageAsync(TemplateWorkflowStage1CAUKStageMessage message, CancellationToken cancellationToken)
    {
        return HandleAsync(message, cancellationToken);
    }
}

partial class TemplateWorkflowStage2BPTGStageHandlerBase : ICapSubscribe
{
    /// <summary>
    /// 处理消息 <inheritdoc cref="TemplateWorkflowStage2BPTGStageMessage.EventName"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [CapSubscribe(TemplateWorkflowStage2BPTGStageMessage.EventName)]
    public virtual Task HandleMessageAsync(TemplateWorkflowStage2BPTGStageMessage message, CancellationToken cancellationToken)
    {
        return HandleAsync(message, cancellationToken);
    }
}

partial class TemplateWorkflowStage3AWBNStageHandlerBase : ICapSubscribe
{
    /// <summary>
    /// 处理消息 <inheritdoc cref="TemplateWorkflowStage3AWBNStageMessage.EventName"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [CapSubscribe(TemplateWorkflowStage3AWBNStageMessage.EventName)]
    public virtual Task HandleMessageAsync(TemplateWorkflowStage3AWBNStageMessage message, CancellationToken cancellationToken)
    {
        return HandleAsync(message, cancellationToken);
    }
}
