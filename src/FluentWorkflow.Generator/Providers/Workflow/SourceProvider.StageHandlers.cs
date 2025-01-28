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

namespace {NameSpace}.Handler;

/// <summary>
/// 阶段完成器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface I{WorkflowClassName}StageFinalizer
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
public interface I{WorkflowClassName}{stage.Name}StageFinalizer : I{WorkflowClassName}StageFinalizer
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
public abstract partial class {WorkflowClassName}StageHandler<TStage, TStageMessage, TStageCompletedMessage>
    : WorkflowStageHandler<TStage, {WorkflowClassName}Context, TStageMessage, TStageCompletedMessage, I{WorkflowClassName}>
    , I{WorkflowClassName}
    , IWorkflowStageHandler<TStageMessage>
    , I{WorkflowClassName}StageFinalizer
    , ICurrentStage
    where TStage : I{WorkflowClassName}
    where TStageMessage : {WorkflowClassName}StageMessageBase, TStage, IEventNameDeclaration
    where TStageCompletedMessage : {WorkflowClassName}StageCompletedMessageBase, TStage, IEventNameDeclaration
{{
    /// <inheritdoc cref=""{WorkflowClassName}StageHandler{{TStage, TStageMessage, TStageCompletedMessage}}""/>
    public {WorkflowClassName}StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override Task StageHandleFailedAsync(TStageMessage stageMessage, Exception exception, CancellationToken cancellationToken)
    {{
        stageMessage.Context.SetFailureMessage(exception.Message);
        stageMessage.Context.SetFailureStackTrace(exception.StackTrace ?? new StackTrace(1, fNeedFileInfo: true).ToString());

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
            MergeContext(typedContext, context);
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

        var failureMessage = context.TryGetFailureMessage(out var failureMessageValue) ? failureMessageValue : ""Unknown error"";
        var failureStackTrace = context.TryGetFailureStackTrace(out var failureStackTraceValue) ? failureStackTraceValue : null;

        var workflowFailureMessage = new {WorkflowClassName}FailureMessage(typedContext, failureMessage, failureStackTrace);
        await MessageDispatcher.PublishAsync(workflowFailureMessage, cancellationToken);
    }}

    #endregion IWorkflowStageFinalizer
}}
");
        var allStageflowDesc = string.Join(" -><br/> ", Context.Stages.Select(m => $"<see cref=\"{WorkflowClassName}Stages.{m.Name}\"/>"));

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"/// <summary>
/// 阶段 <see cref=""{WorkflowClassName}Stages.{stage.Name}""/> 处理器基类<br/>
/// 工作流程阶段顺序：<br/>{allStageflowDesc}
/// </summary>
public abstract partial class {WorkflowClassName}{stage.Name}StageHandlerBase
    : {WorkflowClassName}StageHandler<I{WorkflowClassName}{stage.Name}Stage, {WorkflowClassName}{stage.Name}StageMessage, {WorkflowClassName}{stage.Name}StageCompletedMessage>
    , I{WorkflowClassName}{stage.Name}StageFinalizer
{{
    /// <inheritdoc/>
    public sealed override string Stage {{ get; }} = {WorkflowClassName}Stages.{stage.Name};

    /// <inheritdoc cref=""{WorkflowClassName}{stage.Name}StageHandlerBase""/>
    public {WorkflowClassName}{stage.Name}StageHandlerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {{
    }}

    /// <inheritdoc/>
    protected override {WorkflowClassName}{stage.Name}StageCompletedMessage CreateCompletedMessage({WorkflowClassName}Context context)
    {{
        return new {WorkflowClassName}{stage.Name}StageCompletedMessage(context);
    }}
}}
");
        }

        yield return new($"{WorkflowClassName}.StageHandlers.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
