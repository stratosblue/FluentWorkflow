using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class BaseSourceProvider(GenerateContext generateContext)
    : WorkflowSourceProvider(generateContext)
{
    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(2048);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}
{{
/// <summary>
/// <see cref=""{WorkflowClassName}""/> 基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class {WorkflowClassName}Base
    : IWorkflow
    , I{WorkflowClassName}
    , IWorkflowNameDeclaration
    , IWorkflowContextCarrier<{WorkflowContextName}>
{{
    /// <summary>
    /// 工作流程名称 - {WorkflowClassName}
    /// </summary>
    public const string WorkflowName = ""{WorkflowName}"";

    /// <inheritdoc cref=""WorkflowName""/>
    static string IWorkflowNameDeclaration.WorkflowName => WorkflowName;

    /// <inheritdoc/>
    public {WorkflowContextName} Context {{ get; }}

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public string Id {{ get; }}

    /// <inheritdoc cref=""IServiceProvider""/>
    internal protected readonly IServiceProvider ServiceProvider;

    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc cref=""{WorkflowClassName}Base""/>
    protected {WorkflowClassName}Base({WorkflowContextName} context, IServiceProvider serviceProvider)
    {{
        Context = context ?? throw new ArgumentNullException(nameof(context));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Id = context.Id;
    }}

    /// <summary>
    /// 在工作流程启动时
    /// </summary>
    /// <param name=""context""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns>返回 <see langword=""false""/> 则停止工作流程运行</returns>
    protected virtual Task<bool> OnStartingAsync({WorkflowContextName} context, CancellationToken cancellationToken)
    {{
        return Task.FromResult(true);
    }}
");

        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"    /// <summary>
    /// 在阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 发起前
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""fireMessage"">执行消息后续处理的委托 (分发消息)</param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    protected virtual Task On{stage.Name}Async({Names.MessageName(stage)} message, MessageFireDelegate<{Names.MessageName(stage)}> fireMessage, CancellationToken cancellationToken)
    {{
        return fireMessage(message, cancellationToken);
    }}

    /// <summary>
    /// 在阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 完成时
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""fireMessage"">执行消息后续处理的委托 (更新上下文状态，并分发下阶段消息)</param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    protected virtual Task On{stage.Name}CompletedAsync({Names.CompletedMessageName(stage)} message, MessageFireDelegate<{Names.CompletedMessageName(stage)}> fireMessage, CancellationToken cancellationToken)
    {{
        return fireMessage(message, cancellationToken);
    }}");
        }

        builder.AppendLine($@"
    /// <summary>
    /// 在 <see cref=""{WorkflowClassName}""/> 失败时
    /// </summary>
    /// <param name=""message""></param>
    /// <param name=""fireMessage"">执行消息后续处理的委托 (更新上下文状态，并分发消息)</param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    protected virtual Task OnFailedAsync({Names.FailureMessage} message, MessageFireDelegate<{Names.FailureMessage}> fireMessage, CancellationToken cancellationToken)
    {{
        return fireMessage(message, cancellationToken);
    }}

    /// <summary>
    /// 在 <see cref=""{WorkflowClassName}""/> 完成时
    /// </summary>
    /// <param name=""context""></param>
    /// <param name=""cancellationToken""></param>
    /// <returns></returns>
    protected virtual Task OnCompletionAsync({WorkflowContextName} context, CancellationToken cancellationToken)
    {{
        return Task.CompletedTask;
    }}
}}

partial class {WorkflowClassName}
    : {WorkflowClassName}Base
    , IWorkflowStarter
{{
    #region WorkflowStarter

    /// <inheritdoc/>
    IWorkflow IWorkflowStarter.Workflow => this;

    /// <inheritdoc/>
    Task IWorkflowStarter.StartAsync(CancellationToken cancellationToken)
    {{
        var logger = ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(""FluentWorkflow"");

        var workflowScheduler = ServiceProvider.GetService<IWorkflowScheduler<{WorkflowClassName}>>();
        if (workflowScheduler is not null)
        {{
            logger?.LogInformation(""Start workflow [{{Workflow}}] - {{WorkflowId}} by scheduler."", GetType(), Id);
            return workflowScheduler.StartAsync(this, cancellationToken);
        }}
        else
        {{
            var messageDispatcher = ServiceProvider.GetRequiredService<IWorkflowMessageDispatcher>();
            var startRequestMessage = new {Names.StartRequestMessage}(WorkflowMessageIdProvider.Generate(), Context);
            logger?.LogInformation(""Start workflow [{{Workflow}}] - {{WorkflowId}} by publish start request message."", GetType(), Id);
            return messageDispatcher.PublishAsync(startRequestMessage, cancellationToken);
        }}
    }}

    #endregion WorkflowStarter

    #region Serialize & Resume

    /// <inheritdoc cref=""WorkflowSerializeResumeUtil.SerializeContext{{TWorkflowContext}}(TWorkflowContext, IServiceProvider)""/>
    protected byte[] SerializeContext({WorkflowContextName} context) => WorkflowSerializeResumeUtil.SerializeContext(context, ServiceProvider);

    /// <inheritdoc cref=""WorkflowSerializeResumeUtil.ResumeAsync{{TWorkflow, TWorkflowContext}}(byte[], IServiceProvider, CancellationToken)""/>
    public static Task ResumeAsync(byte[] serializedContext, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {{
        return WorkflowSerializeResumeUtil.ResumeAsync<{WorkflowClassName}, {WorkflowContextName}>(serializedContext, serviceProvider, cancellationToken);
    }}

    #endregion Serialize & Resume
}}
}}

namespace {NameSpace}.{WorkflowName} {{ }}
namespace {NameSpace}.{WorkflowName}.Continuator {{ }}
namespace {NameSpace}.{WorkflowName}.Message {{ }}
namespace {NameSpace}.{WorkflowName}.Handler {{ }}
namespace {NameSpace}.{WorkflowName}.Internal {{ }}
");
        yield return new($"Workflow.{WorkflowName}.Base.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
