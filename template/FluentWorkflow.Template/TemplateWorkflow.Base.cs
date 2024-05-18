﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Interface;
using FluentWorkflow.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TemplateNamespace.Message;

namespace TemplateNamespace;

/// <summary>
/// <see cref="TemplateWorkflow"/> 基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateWorkflowBase
    : IWorkflow
    , ITemplateWorkflow
    , IWorkflowNameDeclaration
    , IWorkflowContextCarrier<TemplateWorkflowContext>
{
    /// <summary>
    /// 工作流程名称 - TemplateWorkflow
    /// </summary>
    public const string WorkflowName = "TemplateWorkflow";

    /// <inheritdoc cref="WorkflowName"/>
    static string IWorkflowNameDeclaration.WorkflowName => WorkflowName;

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc cref="IServiceProvider"/>
    internal protected readonly IServiceProvider ServiceProvider;

    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc cref="TemplateWorkflowBase"/>
    protected TemplateWorkflowBase(TemplateWorkflowContext context, IServiceProvider serviceProvider)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// 声明 <see cref="TemplateWorkflow"/> 的步骤
    /// </summary>
    /// <param name="stageBuilder"></param>
    protected abstract void BuildStages(ITemplateWorkflowStageBuilder stageBuilder);

    /// <summary>
    /// 在阶段 <see cref="TemplateWorkflowStages.Stage1CAUK"/> 发起前
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fireMessage">执行消息后续处理的委托 (分发消息)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnStage1CAUKAsync(TemplateWorkflowStage1CAUKStageMessage message, MessageFireDelegate<TemplateWorkflowStage1CAUKStageMessage> fireMessage, CancellationToken cancellationToken)
    {
        return fireMessage(message, cancellationToken);
    }

    /// <summary>
    /// 在阶段 <see cref="TemplateWorkflowStages.Stage1CAUK"/> 完成时
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fireMessage">执行消息后续处理的委托 (更新上下文状态，并分发下阶段消息)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnStage1CAUKCompletedAsync(TemplateWorkflowStage1CAUKStageCompletedMessage message, MessageFireDelegate<TemplateWorkflowStage1CAUKStageCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        return fireMessage(message, cancellationToken);
    }

    /// <summary>
    /// 在阶段 <see cref="TemplateWorkflowStages.Stage2BPTG"/> 发起前
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fireMessage">执行消息后续处理的委托 (分发消息)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnStage2BPTGAsync(TemplateWorkflowStage2BPTGStageMessage message, MessageFireDelegate<TemplateWorkflowStage2BPTGStageMessage> fireMessage, CancellationToken cancellationToken)
    {
        return fireMessage(message, cancellationToken);
    }

    /// <summary>
    /// 在阶段 <see cref="TemplateWorkflowStages.Stage2BPTG"/> 完成时
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fireMessage">执行消息后续处理的委托 (更新上下文状态，并分发下阶段消息)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnStage2BPTGCompletedAsync(TemplateWorkflowStage2BPTGStageCompletedMessage message, MessageFireDelegate<TemplateWorkflowStage2BPTGStageCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        return fireMessage(message, cancellationToken);
    }

    /// <summary>
    /// 在阶段 <see cref="TemplateWorkflowStages.Stage3AWBN"/> 发起前
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fireMessage">执行消息后续处理的委托 (分发消息)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnStage3AWBNAsync(TemplateWorkflowStage3AWBNStageMessage message, MessageFireDelegate<TemplateWorkflowStage3AWBNStageMessage> fireMessage, CancellationToken cancellationToken)
    {
        return fireMessage(message, cancellationToken);
    }

    /// <summary>
    /// 在阶段 <see cref="TemplateWorkflowStages.Stage3AWBN"/> 完成时
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fireMessage">执行消息后续处理的委托 (更新上下文状态，并分发下阶段消息)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnStage3AWBNCompletedAsync(TemplateWorkflowStage3AWBNStageCompletedMessage message, MessageFireDelegate<TemplateWorkflowStage3AWBNStageCompletedMessage> fireMessage, CancellationToken cancellationToken)
    {
        return fireMessage(message, cancellationToken);
    }

    /// <summary>
    /// 在 <see cref="TemplateWorkflow"/> 失败时
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fireMessage">执行消息后续处理的委托 (更新上下文状态，并分发消息)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnFailedAsync(TemplateWorkflowFailureMessage message, MessageFireDelegate<TemplateWorkflowFailureMessage> fireMessage, CancellationToken cancellationToken)
    {
        return fireMessage(message, cancellationToken);
    }

    /// <summary>
    /// 在 <see cref="TemplateWorkflow"/> 完成时
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnCompletionAsync(TemplateWorkflowContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// 
/// </summary>
partial class TemplateWorkflow
    : TemplateWorkflowBase
    , IWorkflowStarter
{
    #region WorkflowStarter

    /// <inheritdoc/>
    IWorkflow IWorkflowStarter.Workflow => this;

    /// <inheritdoc/>
    Task IWorkflowStarter.StartAsync(CancellationToken cancellationToken)
    {
        var logger = ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("FluentWorkflow");

        var workflowScheduler = ServiceProvider.GetService<IWorkflowScheduler<TemplateWorkflow>>();
        if (workflowScheduler is not null)
        {
            logger?.LogInformation("Start workflow [{Workflow}] - {WorkflowId} by scheduler.", GetType(), Id);
            return workflowScheduler.StartAsync(this, cancellationToken);
        }
        else
        {
            var messageDispatcher = ServiceProvider.GetRequiredService<IWorkflowMessageDispatcher>();
            var startRequestMessage = new TemplateWorkflowStartRequestMessage(Context);
            logger?.LogInformation("Start workflow [{Workflow}] - {WorkflowId} by publish start request message.", GetType(), Id);
            return messageDispatcher.PublishAsync(startRequestMessage, cancellationToken);
        }
    }

    #endregion WorkflowStarter

    #region Serialize & Resume

    /// <inheritdoc cref="WorkflowSerializeResumeUtil.SerializeContext{TWorkflowContext}(TWorkflowContext, IServiceProvider)"/>
    protected byte[] SerializeContext(TemplateWorkflowContext context) => WorkflowSerializeResumeUtil.SerializeContext(context, ServiceProvider);

    /// <inheritdoc cref="WorkflowSerializeResumeUtil.ResumeAsync{TWorkflow, TWorkflowContext}(byte[], IServiceProvider, CancellationToken)"/>
    public static Task ResumeAsync(byte[] serializedContext, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return WorkflowSerializeResumeUtil.ResumeAsync<TemplateWorkflow, TemplateWorkflowContext>(serializedContext, serviceProvider, cancellationToken);
    }

    #endregion Serialize & Resume
}
