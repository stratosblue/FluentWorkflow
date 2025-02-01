﻿// <Auto-Generated/>

using System.ComponentModel;
using System.Diagnostics;
using FluentWorkflow;
using FluentWorkflow.Extensions;
using FluentWorkflow.Handler;
using TemplateNamespace.Template.Message;
using FluentWorkflow.Abstractions;

namespace TemplateNamespace.Template.Handler;

/// <summary>
/// 阶段完成器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITemplateStageFinalizer
    : IWorkflowStageFinalizer, ITemplateWorkflow
{
}

/// <summary>
/// 阶段 Stage1CAUK 完成器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IStageStage1CAUKFinalizer : ITemplateStageFinalizer
{
}

/// <summary>
/// 阶段 Stage2BPTG 完成器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IStageStage2BPTGFinalizer : ITemplateStageFinalizer
{
}

/// <summary>
/// 阶段 Stage3AWBN 完成器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IStageStage3AWBNFinalizer : ITemplateStageFinalizer
{
}

/// <summary>
/// 阶段处理器基类
/// </summary>
/// <typeparam name="TStage"></typeparam>
/// <typeparam name="TStageMessage"></typeparam>
/// <typeparam name="TStageCompletedMessage"></typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateStageHandler<TStage, TStageMessage, TStageCompletedMessage>
    : WorkflowStageHandler<TStage, TemplateWorkflowContext, TStageMessage, TStageCompletedMessage, ITemplateWorkflow>
    , ITemplateWorkflow
    , IWorkflowStageHandler<TStageMessage>
    , ITemplateStageFinalizer
    , ICurrentStage
    where TStage : ITemplateWorkflow
    where TStageMessage : TemplateStageMessageBase, TStage, IEventNameDeclaration
    where TStageCompletedMessage : TemplateStageCompletedMessageBase, TStage, IEventNameDeclaration
{
    /// <inheritdoc cref="TemplateStageHandler{TStage, TStageMessage, TStageCompletedMessage}"/>
    public TemplateStageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override Task StageHandleFailedAsync(TStageMessage stageMessage, Exception exception, CancellationToken cancellationToken)
    {
        stageMessage.Context.SetFailureInformation(stage: stageMessage.Stage,
                                                   message: exception.Message,
                                                   stackTrace: exception.StackTrace ?? new StackTrace(1, fNeedFileInfo: true).ToString());

        return ((IWorkflowStageFinalizer)this).FailAsync(stageMessage.Context, cancellationToken);
    }

    /// <summary>
    /// 子工作流程等待结束（处理合并上下文到主流程上下文）
    /// </summary>
    /// <param name="context"></param>
    /// <param name="childWorkflowContexts"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnAwaitFinishedAsync(TemplateWorkflowContext context, IReadOnlyDictionary<string, IWorkflowContext?> childWorkflowContexts, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    #region IWorkflowStageFinalizer

    /// <inheritdoc/>
    async Task IWorkflowStageFinalizer.AwaitFinishedAsync(IWorkflowContext context, IReadOnlyDictionary<string, IWorkflowContext?> childWorkflowContexts, CancellationToken cancellationToken)
    {
        var typedContext = new TemplateWorkflowContext(context.GetSnapshot());
        try
        {
            await OnAwaitFinishedAsync(typedContext, childWorkflowContexts, cancellationToken);
        }
        finally
        {
            //在完成等待时出现异常也需要将修改反应回原上下文
            //将修改反应回原上下文
            context.ApplyChanges(typedContext);
        }
    }

    /// <inheritdoc/>
    async Task IWorkflowStageFinalizer.CompleteAsync(IWorkflowContext context, CancellationToken cancellationToken)
    {
        ThrowIfStageNotMatch(context);

        var typedContext = new TemplateWorkflowContext(context.GetSnapshot());

        //HACK 包装 OnProcessSuccessAsync 的用户异常，保证消息正确发送？
        await OnProcessSuccessAsync(typedContext, cancellationToken);
        var stageCompletedMessage = CreateCompletedMessage(typedContext);
        await MessageDispatcher.PublishAsync(stageCompletedMessage, cancellationToken);
    }

    /// <inheritdoc/>
    async Task IWorkflowStageFinalizer.FailAsync(IWorkflowContext context, CancellationToken cancellationToken)
    {
        ThrowIfStageNotMatch(context);

        var typedContext = new TemplateWorkflowContext(context.GetSnapshot());

        //HACK 包装 OnProcessFailedAsync 的用户异常，保证消息正确发送？
        await OnProcessFailedAsync(typedContext, cancellationToken);

        var failureInformation = context.GetFailureInformation();
        var failureMessage = failureInformation?.Message ?? "Unknown error";
        var failureStackTrace = failureInformation.StackTrace;

        var workflowFailureMessage = new TemplateFailureMessage(WorkflowMessageIdProvider.Generate(), typedContext, failureMessage, failureStackTrace);
        await MessageDispatcher.PublishAsync(workflowFailureMessage, cancellationToken);
    }

    #endregion IWorkflowStageFinalizer
}

/// <summary>
/// 阶段 <see cref="TemplateStages.Stage1CAUK"/> 处理器基类<br/>
/// 工作流程阶段顺序：<br/><see cref="TemplateStages.Stage1CAUK"/> -><br/> <see cref="TemplateStages.Stage2BPTG"/> -><br/> <see cref="TemplateStages.Stage3AWBN"/>
/// </summary>
public abstract partial class StageStage1CAUKHandlerBase
    : TemplateStageHandler<IStageStage1CAUK, StageStage1CAUKMessage, StageStage1CAUKCompletedMessage>
    , IStageStage1CAUKFinalizer
{
    /// <inheritdoc/>
    public sealed override string Stage { get; } = TemplateStages.Stage1CAUK;

    /// <inheritdoc cref="StageStage1CAUKHandlerBase"/>
    public StageStage1CAUKHandlerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override StageStage1CAUKCompletedMessage CreateCompletedMessage(TemplateWorkflowContext context)
    {
        return new StageStage1CAUKCompletedMessage(WorkflowMessageIdProvider.Generate(), context);
    }
}

/// <summary>
/// 阶段 <see cref="TemplateStages.Stage2BPTG"/> 处理器基类<br/>
/// 工作流程阶段顺序：<br/><see cref="TemplateStages.Stage1CAUK"/> -><br/> <see cref="TemplateStages.Stage2BPTG"/> -><br/> <see cref="TemplateStages.Stage3AWBN"/>
/// </summary>
public abstract partial class StageStage2BPTGHandlerBase
    : TemplateStageHandler<IStageStage2BPTG, StageStage2BPTGMessage, StageStage2BPTGCompletedMessage>
    , IStageStage2BPTGFinalizer
{
    /// <inheritdoc/>
    public sealed override string Stage { get; } = TemplateStages.Stage2BPTG;

    /// <inheritdoc cref="StageStage2BPTGHandlerBase"/>
    public StageStage2BPTGHandlerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override StageStage2BPTGCompletedMessage CreateCompletedMessage(TemplateWorkflowContext context)
    {
        return new StageStage2BPTGCompletedMessage(WorkflowMessageIdProvider.Generate(), context);
    }
}

/// <summary>
/// 阶段 <see cref="TemplateStages.Stage3AWBN"/> 处理器基类<br/>
/// 工作流程阶段顺序：<br/><see cref="TemplateStages.Stage1CAUK"/> -><br/> <see cref="TemplateStages.Stage2BPTG"/> -><br/> <see cref="TemplateStages.Stage3AWBN"/>
/// </summary>
public abstract partial class StageStage3AWBNHandlerBase
    : TemplateStageHandler<IStageStage3AWBN, StageStage3AWBNMessage, StageStage3AWBNCompletedMessage>
    , IStageStage3AWBNFinalizer
{
    /// <inheritdoc/>
    public sealed override string Stage { get; } = TemplateStages.Stage3AWBN;

    /// <inheritdoc cref="StageStage3AWBNHandlerBase"/>
    public StageStage3AWBNHandlerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override StageStage3AWBNCompletedMessage CreateCompletedMessage(TemplateWorkflowContext context)
    {
        return new StageStage3AWBNCompletedMessage(WorkflowMessageIdProvider.Generate(), context);
    }
}
