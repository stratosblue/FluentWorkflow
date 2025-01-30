using System.Collections.Immutable;
using System.ComponentModel;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Extensions;
using FluentWorkflow.Interface;
using FluentWorkflow.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow;

/// <summary>
/// 工作流程阶段处理器
/// </summary>
/// <typeparam name="TStage"></typeparam>
/// <typeparam name="TStageMessage"></typeparam>
/// <typeparam name="TWorkflowContext"></typeparam>
/// <typeparam name="TStageCompletedMessage"></typeparam>
/// <typeparam name="TWorkflowBoundary">工作流程边界（限定工作流程）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class WorkflowStageHandler<TStage, TWorkflowContext, TStageMessage, TStageCompletedMessage, TWorkflowBoundary>
    : IWorkflowStageHandler<TStageMessage>
    , ICurrentStage
    where TStage : TWorkflowBoundary
    where TWorkflowContext : IWorkflowContext, TWorkflowBoundary
    where TStageMessage : IWorkflowStageMessage, IWorkflowContextCarrier<TWorkflowContext>, TStage, TWorkflowBoundary, IEventNameDeclaration
    where TStageCompletedMessage : IWorkflowStageCompletedMessage, TStage, TWorkflowBoundary, IEventNameDeclaration
{
    #region Private 字段

    private readonly IWorkflowDiagnosticSource _diagnosticSource;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public abstract string Stage { get; }

    #endregion Public 属性

    #region Protected 属性

    /// <inheritdoc cref="ILogger"/>
    protected ILogger Logger { get; }

    /// <inheritdoc cref="IWorkflowMessageDispatcher"/>
    protected IWorkflowMessageDispatcher MessageDispatcher { get; }

    /// <inheritdoc cref="IObjectSerializer"/>
    protected IObjectSerializer ObjectSerializer { get; }

    /// <inheritdoc cref="IServiceProvider"/>
    protected IServiceProvider ServiceProvider { get; }

    /// <inheritdoc cref="IWorkflowAwaitProcessor"/>
    protected IWorkflowAwaitProcessor WorkflowAwaitProcessor { get; }

    #endregion Protected 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowStageHandler{TStage, TWorkflowContext, TStageMessage, TStageCompletedMessage, TWorkflowBoundary}"/>
    public WorkflowStageHandler(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        MessageDispatcher = serviceProvider.GetRequiredService<IWorkflowMessageDispatcher>();
        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
        WorkflowAwaitProcessor = serviceProvider.GetRequiredService<IWorkflowAwaitProcessor>();
        ObjectSerializer = serviceProvider.GetRequiredService<IObjectSerializer>();

        _diagnosticSource = serviceProvider.GetRequiredService<IWorkflowDiagnosticSource>();
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 处理消息（当前处理器的入口，内部会处理<see cref="ProcessAsync(ProcessContext, TStageMessage, CancellationToken)"/>失败和成功的后续步骤）
    /// </summary>
    /// <param name="stageMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task HandleAsync(TStageMessage stageMessage, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"{DiagnosticConstants.ActivityNames.StageProcessing} - {stageMessage.Stage}", System.Diagnostics.ActivityKind.Consumer);
        activity?.AddTag(DiagnosticConstants.ActivityNames.TagKeys.Message, PrettyJSONObject.Create(stageMessage, ObjectSerializer));

        Exception? exception = null;
        var notFiredDiagnostic = true;

        try
        {
            _diagnosticSource.StageMessageHandleStart(stageMessage);

            ThrowIfStageNotMatch(stageMessage.Context);

            Logger.LogTrace("Start handle stage message {{{Id}}}[{Message}]", stageMessage.Id, stageMessage);

            var processContext = new ProcessContext(ServiceProvider);

            try
            {
                await ProcessAsync(processContext, stageMessage, cancellationToken);

                //有需要等待的子工作流程
                if (processContext.AwaitChildWorkflows.Count > 0)
                {
                    var childWorkflow = processContext.AwaitChildWorkflows.ToArray();

                    var childWorkflowCount = childWorkflow.Select(static m => m.Value.Workflow).Distinct(WorkflowComparer.Instance).Count();

                    if (childWorkflowCount != childWorkflow.Length)
                    {
                        throw new WorkflowInvalidOperationException("Do not await child workflow multiple times.");
                    }

                    processContext.CommitChildWorkflow();

                    await WorkflowAwaitProcessor.RegisterAsync(stageMessage.Context, childWorkflow.ToDictionary(m => m.Key, m => m.Value.Workflow), cancellationToken);

                    var childWorkflowFlag = childWorkflowCount == 1 ? WorkflowFlag.UniqueChildWorkflow : WorkflowFlag.None;

                    childWorkflowFlag |= WorkflowFlag.HasParentWorkflow | WorkflowFlag.IsBeenAwaited;

                    WorkflowContextSnapshot? currentContextSnapshot = null;
                    foreach (var (alias, workflowStarter) in childWorkflow)
                    {
                        var workflow = workflowStarter.Workflow;
                        workflow.Context.State.Alias = alias;
                        workflow.Context.SetParent(currentContextSnapshot ??= new WorkflowContextSnapshot(stageMessage.Context.GetSnapshot()));
                        workflow.Context.Flag |= childWorkflowFlag;
                    }

                    foreach (var (_, workflowStarter) in childWorkflow)
                    {
                        await workflowStarter.StartAsync(cancellationToken);
                    }

                    //有需要等待的子工作流程，不进行自动确认
                    return;
                }
            }
            catch (Exception ex)
            {
                if (OnException(ex))
                {
                    await StageHandleFailedAsync(stageMessage, ex, cancellationToken);
                    exception = ex;
                    Logger.LogError(ex, "Handle stage message {{{Id}}}[{Message}] failed", stageMessage.Id, stageMessage);
                    return;
                }
                throw;
            }

            if (!processContext.DisableAutoACKStageCompleted)
            {
                await StageHandleCompletedAsync(stageMessage, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            _diagnosticSource.StageMessageHandleEnd(stageMessage, false, ex);
            notFiredDiagnostic = false;
            throw;
        }
        finally
        {
            if (notFiredDiagnostic)
            {
                _diagnosticSource.StageMessageHandleEnd(stageMessage, true, exception);
            }
        }
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 创建阶段对应的完成消息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected abstract TStageCompletedMessage CreateCompletedMessage(TWorkflowContext context);

    /// <summary>
    /// 在 <see cref="ProcessAsync(ProcessContext, TStageMessage, CancellationToken)"/> 中出现异常
    /// </summary>
    /// <param name="exception"></param>
    /// <returns>
    /// <see langword="true"/>：则吞没异常，认为工作流程已失败，发送工作流程失败消息<br/>
    /// <see langword="false"/>：则不认为工作流程失败，不会发送工作流程失败消息，会将异常抛出，由上层进行处理<br/>
    /// </returns>
    protected virtual bool OnException(Exception exception)
    {
        return true;
    }

    /// <summary>
    /// <see cref="ProcessAsync(ProcessContext, TStageMessage, CancellationToken)"/> 执行失败，认为工作流程已失败
    /// <br/>(此方法内不应该抛出异常，会导致消息重新消费)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnProcessFailedAsync(TWorkflowContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// <see cref="ProcessAsync(ProcessContext, TStageMessage, CancellationToken)"/> 执行成功
    /// <br/>(此方法内不应该抛出异常，会导致消息重新消费)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnProcessSuccessAsync(TWorkflowContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// 阶段的业务处理
    /// </summary>
    /// <param name="processContext"></param>
    /// <param name="stageMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task ProcessAsync(ProcessContext processContext, TStageMessage stageMessage, CancellationToken cancellationToken);

    /// <summary>
    /// 在 <see cref="HandleAsync(TStageMessage, CancellationToken)"/> 执行完成后执行的方法
    /// </summary>
    /// <param name="stageMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task StageHandleCompletedAsync(TStageMessage stageMessage, CancellationToken cancellationToken)
    {
        return ((IWorkflowStageFinalizer)this).CompleteAsync(stageMessage.Context, cancellationToken);
    }

    /// <summary>
    /// 在 <see cref="HandleAsync(TStageMessage, CancellationToken)"/> 执行出现异常后且认为工作流程已失败时执行的方法
    /// </summary>
    /// <param name="stageMessage"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task StageHandleFailedAsync(TStageMessage stageMessage, Exception exception, CancellationToken cancellationToken);

    /// <summary>
    /// 当<paramref name="context"/>的阶段和当前阶段处理器的阶段不对应时，抛出异常
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="WorkflowInvalidOperationException"></exception>
    protected void ThrowIfStageNotMatch(IWorkflowContext context)
    {
        if (!string.Equals(Stage, context.Stage))
        {
            throw new WorkflowInvalidOperationException($"Stage \"{context.Stage}\" not match for current handler.");
        }
    }

    #endregion Protected 方法

    #region Protected 类

    /// <summary>
    /// 处理上下文
    /// </summary>
    protected sealed class ProcessContext
    {
        #region Private 字段

        private readonly IServiceProvider _serviceProvider;

        private ImmutableDictionary<string, IWorkflowStarter> _awaitChildWorkflows = ImmutableDictionary.Create<string, IWorkflowStarter>();

        /// <summary>
        /// 是否已提交
        /// </summary>
        private bool _hasCommitted = false;

        private IServiceProviderIsService? _serviceProviderIsService;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 需要等待的子任务
        /// </summary>
        public IReadOnlyDictionary<string, IWorkflowStarter> AwaitChildWorkflows => _awaitChildWorkflows;

        /// <summary>
        /// 禁用自动确认阶段完成<br/>
        /// 设置为 <see langword="true"/> 时，不会自动发送阶段完成消息
        /// </summary>
        public bool DisableAutoACKStageCompleted { get; set; } = false;

        #endregion Public 属性

        #region Private 属性

        private IServiceProviderIsService ServiceProviderIsService => _serviceProviderIsService ??= _serviceProvider.GetRequiredService<IServiceProviderIsService>();

        #endregion Private 属性

        #region Public 构造函数

        /// <inheritdoc cref="ProcessContext"/>
        public ProcessContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <summary>
        /// 添加等待子工作流程（子工作流程需要为未启动状态，当前阶段完成时会自动启动）
        /// </summary>
        /// <typeparam name="TWorkflow"></typeparam>
        /// <param name="workflow"></param>
        /// <exception cref="WorkflowInvalidOperationException"></exception>
        public void AwaitChildWorkflow<TWorkflow>(TWorkflow workflow)
            where TWorkflow : IWorkflow, IWorkflowStarter
        {
            ThrowIfCommitted();

            AwaitChildWorkflow<TWorkflow>(Guid.NewGuid().ToString(), workflow);
        }

        /// <summary>
        /// 添加具名等待子工作流程（子工作流程需要为未启动状态，当前阶段完成时会自动启动）
        /// </summary>
        /// <typeparam name="TWorkflow"></typeparam>
        /// <param name="alias">子工作流程唯一别名</param>
        /// <param name="workflow"></param>
        /// <exception cref="WorkflowInvalidOperationException"></exception>
        public void AwaitChildWorkflow<TWorkflow>(string alias, TWorkflow workflow)
            where TWorkflow : IWorkflow, IWorkflowStarter
        {
            ThrowIfCommitted();

            WorkflowException.ThrowIfNullOrWhiteSpace(alias);

            //检查当前服务是否有对应工作流程的结果观察器，否则不允许启动
            if (!ServiceProviderIsService.IsService(typeof(IWorkflowResultObserver<TWorkflow>)))
            {
                throw new WorkflowInvalidOperationException($"There is no {nameof(IWorkflowResultObserver<TWorkflow>)} for \"{typeof(TWorkflow)}\" in serviceProvider. Can not await child workflow.");
            }
            _awaitChildWorkflows = _awaitChildWorkflows.Add(alias, workflow);
        }

        /// <summary>
        /// 提交子工作流程（不再可以进行添加）
        /// </summary>
        public void CommitChildWorkflow()
        {
            _hasCommitted = true;
        }

        #endregion Public 方法

        #region Private 方法

        private void ThrowIfCommitted()
        {
            if (_hasCommitted)
            {
                throw new WorkflowInvalidOperationException("Child workflows has committed. Can not operate now.");
            }
        }

        #endregion Private 方法
    }

    #endregion Protected 类
}
