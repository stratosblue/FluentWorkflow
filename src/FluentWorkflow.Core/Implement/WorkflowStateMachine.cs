using System.ComponentModel;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

/// <summary>
/// 工作流程状态机
/// </summary>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class WorkflowStateMachine<TWorkflowBoundary>
    : IWorkflowStateMachine
{
    #region Private 字段

    private readonly IWorkflowDiagnosticSource _diagnosticSource;

    #endregion Private 字段

    #region Protected 字段

    /// <inheritdoc cref="IWorkflowMessageDispatcher"/>
    protected readonly IWorkflowMessageDispatcher MessageDispatcher;

    #endregion Protected 字段

    #region Public 属性

    /// <inheritdoc/>
    public IWorkflowContext Context { get; }

    /// <inheritdoc/>
    public string Id => Context.Id;

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="WorkflowStateMachine{TWorkflowBoundary}"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="messageDispatcher"></param>
    /// <param name="serviceProvider"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public WorkflowStateMachine(IWorkflowContext context, IWorkflowMessageDispatcher messageDispatcher, IServiceProvider serviceProvider)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        MessageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));

        _diagnosticSource = serviceProvider.GetRequiredService<IWorkflowDiagnosticSource>();
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public abstract Task<bool> IsCompletedAsync(CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task MoveNextAsync(CancellationToken cancellationToken);

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 在阶段完成时
    /// </summary>
    /// <param name="stageCompletedMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnStageCompletedAsync<TStageCompletedMessage>(TStageCompletedMessage stageCompletedMessage, CancellationToken cancellationToken)
        where TStageCompletedMessage : IWorkflowStageCompletedMessage, TWorkflowBoundary
    {
        _diagnosticSource.WorkflowStageCompleted(stageCompletedMessage);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 发布阶段消息
    /// </summary>
    /// <typeparam name="TStageMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task PublishStageMessageAsync<TStageMessage>(TStageMessage message, CancellationToken cancellationToken)
        where TStageMessage : class, IWorkflowStageMessage, IWorkflowContextCarrier<IWorkflowContext>, TWorkflowBoundary, IEventNameDeclaration
    {
        return MessageDispatcher.PublishAsync(message, cancellationToken);
    }

    #endregion Protected 方法
}
