using System.ComponentModel;
using System.Diagnostics;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Extensions;
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
    /// 在工作流程失败时
    /// </summary>
    /// <param name="failureMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnFailedAsync<TFailureMessage>(TFailureMessage failureMessage, CancellationToken cancellationToken)
        where TFailureMessage : IWorkflowFailureMessage, TWorkflowBoundary
    {
        Context.SetFailureInformation(failureMessage.Stage,failureMessage.Message,failureMessage.RemoteStackTrace);

        return Task.CompletedTask;
    }

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

    #region Private 类

    /// <summary>
    /// 范围限定的单次失败调用器
    /// </summary>
    /// <param name="stateMachine"></param>
    protected sealed class ScopeOnFailedSingleCaller(WorkflowStateMachine<TWorkflowBoundary> stateMachine) : ScopeSingleCaller
    {
        #region Public 方法

        /// <inheritdoc cref="WorkflowStateMachine{TWorkflowBoundary}.OnStageCompletedAsync{TStageCompletedMessage}(TStageCompletedMessage, CancellationToken)"/>
        [DebuggerStepThrough]
        public Task OnFailedAsync<TFailureMessage>(TFailureMessage failureMessage, CancellationToken cancellationToken)
            where TFailureMessage : IWorkflowFailureMessage, TWorkflowBoundary
        {
            InvokeCheck();
            return stateMachine.OnFailedAsync(failureMessage, cancellationToken);
        }

        #endregion Public 方法
    }

    /// <summary>
    /// 范围限定的单次阶段完成调用器
    /// </summary>
    /// <param name="stateMachine"></param>
    protected sealed class ScopeOnStageCompletedSingleCaller(WorkflowStateMachine<TWorkflowBoundary> stateMachine) : ScopeSingleCaller
    {
        #region Public 方法

        /// <inheritdoc cref="WorkflowStateMachine{TWorkflowBoundary}.OnStageCompletedAsync{TStageCompletedMessage}(TStageCompletedMessage, CancellationToken)"/>
        [DebuggerStepThrough]
        public Task OnStageCompletedAsync<TStageCompletedMessage>(TStageCompletedMessage stageCompletedMessage, CancellationToken cancellationToken)
            where TStageCompletedMessage : IWorkflowStageCompletedMessage, TWorkflowBoundary
        {
            InvokeCheck();
            return stateMachine.OnStageCompletedAsync(stageCompletedMessage, cancellationToken);
        }

        #endregion Public 方法
    }

    /// <summary>
    /// 范围限定的单次发布阶段消息调用器
    /// </summary>
    /// <param name="messageDispatcher"></param>
    protected sealed class ScopePublishStageMessageSingleCaller(IWorkflowMessageDispatcher messageDispatcher) : ScopeSingleCaller
    {
        #region Public 方法

        /// <inheritdoc cref="WorkflowStateMachine{TWorkflowBoundary}.PublishStageMessageAsync{TStageMessage}(TStageMessage, CancellationToken)"/>
        [DebuggerStepThrough]
        public Task PublishStageMessageAsync<TStageMessage>(TStageMessage message, CancellationToken cancellationToken)
            where TStageMessage : class, IWorkflowStageMessage, IWorkflowContextCarrier<IWorkflowContext>, TWorkflowBoundary, IEventNameDeclaration
        {
            InvokeCheck();

            return messageDispatcher.PublishAsync(message, cancellationToken);
        }

        #endregion Public 方法
    }

    /// <summary>
    /// 范围限定的单次调用器
    /// </summary>
    protected abstract class ScopeSingleCaller : IDisposable
    {
        #region Private 字段

        private volatile bool _hasInvoked = false;

        private int _invocationFlag = 0;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 是否已调用
        /// </summary>
        public bool HasInvoked => _hasInvoked;

        #endregion Public 属性

        #region Public 方法

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public void Dispose()
        {
            _invocationFlag = 1;
        }

        #endregion Public 方法

        #region Protected 方法

        /// <summary>
        /// 进行执行检查
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        [DebuggerStepThrough]
        protected void InvokeCheck()
        {
            if (Interlocked.CompareExchange(ref _invocationFlag, 1, 0) != 0)
            {
                throw new InvalidOperationException("Invalid operation. Do not call the delegate multiple or call it after method finished.");
            }

            _hasInvoked = true;
        }

        #endregion Protected 方法
    }

    #endregion Private 类
}
