using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StageHandlerSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public StageHandlerSourceProvider(GenerateContext context) : base(context)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(2048);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}.{WorkflowName}.Handler;

/// <summary>
/// 阶段完成器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface I{WorkflowName}StageFinalizer
    : IWorkflowStageFinalizer, I{WorkflowClassName}
{{
}}
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"/// <summary>
/// 阶段 {stage.Name} 完成器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IStage{stage.Name}Finalizer : I{WorkflowName}StageFinalizer
{{
}}");
        }

        builder.AppendLine($@"/// <summary>
/// 阶段处理器基类
/// </summary>
/// <typeparam name=""TStage""></typeparam>
/// <typeparam name=""TStageMessage""></typeparam>
/// <typeparam name=""TStageCompletedMessage""></typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowName}StageHandler<TStage, TStageMessage, TStageCompletedMessage>
    : WorkflowStageHandler<TStage, {WorkflowClassName}Context, TStageMessage, TStageCompletedMessage, I{WorkflowClassName}>
    , I{WorkflowClassName}
    , IWorkflowStageHandler<TStageMessage>
    , I{WorkflowName}StageFinalizer
    , ICurrentStage
    where TStage : I{WorkflowClassName}
    where TStageMessage : {WorkflowName}StageMessageBase, TStage, IEventNameDeclaration
    where TStageCompletedMessage : {WorkflowName}StageCompletedMessageBase, TStage, IEventNameDeclaration
{{
    /// <inheritdoc cref=""{WorkflowName}StageHandler{{TStage, TStageMessage, TStageCompletedMessage}}""/>
    public {WorkflowName}StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override Task StageHandleFailedAsync(TStageMessage stageMessage, Exception exception, CancellationToken cancellationToken)
    {{
        stageMessage.Context.SetFailureInformation(stage: stageMessage.Stage,
                                                   message: exception.Message,
                                                   stackTrace: exception.StackTrace ?? new StackTrace(1, fNeedFileInfo: true).ToString());

        return ((IWorkflowStageFinalizer)this).FailAsync(stageMessage.Context, cancellationToken);
    }}

    /// <summary>
    /// 子工作流程等待结束（处理合并上下文到主流程上下文）
    /// </summary>
    /// <param name=""context""></param>
    /// <param name=""childWorkflowContexts""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    protected virtual Task OnAwaitFinishedAsync({WorkflowClassName}Context context, IReadOnlyDictionary<string, IWorkflowContext?> childWorkflowContexts, CancellationToken cancellationToken)
    {{
        return Task.CompletedTask;
    }}

    #region IWorkflowStageFinalizer

    /// <inheritdoc/>
    async Task IWorkflowStageFinalizer.AwaitFinishedAsync(IWorkflowContext context, IReadOnlyDictionary<string, IWorkflowContext?> childWorkflowContexts, CancellationToken cancellationToken)
    {{
        var typedContext = new {WorkflowClassName}Context(context.GetSnapshot());

        try
        {{
            await OnAwaitFinishedAsync(typedContext, childWorkflowContexts, cancellationToken);
        }}
        finally
        {{
            //在完成等待时出现异常也需要将修改反应回原上下文
            //将修改反应回原上下文
            context.ApplyChanges(typedContext);
        }}
    }}

    /// <inheritdoc/>
    async Task IWorkflowStageFinalizer.CompleteAsync(IWorkflowContext context, CancellationToken cancellationToken)
    {{
        ThrowIfStageNotMatch(context);

        var typedContext = new {WorkflowClassName}Context(context.GetSnapshot());

        await OnProcessSuccessAsync(typedContext, cancellationToken);
        var stageCompletedMessage = CreateCompletedMessage(typedContext);
        await MessageDispatcher.PublishAsync(stageCompletedMessage, cancellationToken);
    }}

    /// <inheritdoc/>
    async Task IWorkflowStageFinalizer.FailAsync(IWorkflowContext context, CancellationToken cancellationToken)
    {{
        ThrowIfStageNotMatch(context);

        var typedContext = new {WorkflowClassName}Context(context.GetSnapshot());

        await OnProcessFailedAsync(typedContext, cancellationToken);

        var failureInformation = context.GetFailureInformation();
        var failureMessage = failureInformation?.Message ?? ""Unknown error"";
        var failureStackTrace = failureInformation.StackTrace;

        var workflowFailureMessage = new {WorkflowName}FailureMessage(WorkflowMessageIdProvider.Generate(), typedContext, failureMessage, failureStackTrace);
        await MessageDispatcher.PublishAsync(workflowFailureMessage, cancellationToken);
    }}

    #endregion IWorkflowStageFinalizer
}}
");
        var allStageflowDesc = string.Join(" -><br/> ", Context.Stages.Select(m => $"<see cref=\"{WorkflowName}Stages.{m.Name}\"/>"));

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"/// <summary>
/// 阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 处理器基类<br/>
/// 工作流程阶段顺序：<br/>{allStageflowDesc}
/// </summary>
public abstract partial class Stage{stage.Name}HandlerBase
    : {WorkflowName}StageHandler<IStage{stage.Name}, Stage{stage.Name}Message, Stage{stage.Name}CompletedMessage>
    , IStage{stage.Name}Finalizer
{{
    /// <inheritdoc/>
    public sealed override string Stage {{ get; }} = {WorkflowName}Stages.{stage.Name};

    /// <inheritdoc cref=""Stage{stage.Name}HandlerBase""/>
    public Stage{stage.Name}HandlerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override Stage{stage.Name}CompletedMessage CreateCompletedMessage({WorkflowClassName}Context context)
    {{
        return new Stage{stage.Name}CompletedMessage(WorkflowMessageIdProvider.Generate(), context);
    }}
}}
");
        }

        yield return new($"Workflow.{WorkflowName}.StageHandlers.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
