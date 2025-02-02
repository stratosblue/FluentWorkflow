using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class MessagesSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public MessagesSourceProvider(GenerateContext context) : base(context)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(2048);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using System.Text.Json.Serialization;

namespace {NameSpace}.{WorkflowName}.Message;

/// <summary>
/// <see cref=""{WorkflowClassName}Context""/> 携带者接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowName}ContextCarrier
    : IWorkflowContextCarrier<{WorkflowClassName}Context>
    , I{WorkflowClassName}
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 启动请求消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowName}StartRequestMessage : IWorkflowStartRequestMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程结束消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowName}FinishedMessage : IWorkflowFinishedMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 阶段消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowName}StageMessage : IWorkflowStageMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 阶段完成消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowName}StageCompletedMessage : IWorkflowStageCompletedMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程失败消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowName}FailureMessage : IWorkflowFailureMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 阶段消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public abstract partial class {WorkflowName}StageMessageBase : I{WorkflowName}StageMessage
{{
    /// <inheritdoc/>
    public string Id {{ get; }}

    /// <inheritdoc/>
    [JsonIgnore]
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}StageMessageBase""/>
    protected {WorkflowName}StageMessageBase(string id, {WorkflowClassName}Context context)
    {{
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 阶段完成消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public abstract partial class {WorkflowName}StageCompletedMessageBase : I{WorkflowName}StageCompletedMessage
{{
    /// <inheritdoc/>
    public string Id {{ get; }}

    /// <inheritdoc/>
    [JsonIgnore]
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}StageCompletedMessageBase""/>
    protected {WorkflowName}StageCompletedMessageBase(string id, {WorkflowClassName}Context context)
    {{
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 启动请求消息
/// </summary>
[WorkflowName({WorkflowClassName}.WorkflowName)]
public sealed partial class {WorkflowName}StartRequestMessage : I{WorkflowName}StartRequestMessage
{{
    /// <inheritdoc cref=""{WorkflowClassName}Base.WorkflowName""/>
    public static string WorkflowName => {WorkflowClassName}.WorkflowName;

    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowName}.StartRequest
    /// </summary>
    public const string EventName = ""{WorkflowName}.StartRequest"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc/>
    public string Id {{ get; }}

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc cref=""{WorkflowName}StartRequestMessage""/>
    public {WorkflowName}StartRequestMessage(string id, IEnumerable<KeyValuePair<string, string>> context)
        : this(id, new {WorkflowClassName}Context(context))
    {{
    }}

    /// <inheritdoc cref=""{WorkflowName}StartRequestMessage""/>
    [System.Text.Json.Serialization.JsonConstructor]
    public {WorkflowName}StartRequestMessage(string id, {WorkflowClassName}Context context)
    {{
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程结束消息
/// </summary>
[WorkflowName({WorkflowClassName}.WorkflowName)]
public sealed partial class {WorkflowName}FinishedMessage : I{WorkflowName}FinishedMessage
{{
    /// <inheritdoc cref=""{WorkflowClassName}Base.WorkflowName""/>
    public static string WorkflowName => {WorkflowClassName}.WorkflowName;

    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowName}.Finished
    /// </summary>
    public const string EventName = ""{WorkflowName}.Finished"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc/>
    public string Id {{ get; }}

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc/>
    public bool IsSuccess {{ get; }}

    /// <inheritdoc/>
    public string? Message {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}FinishedMessage""/>
    public {WorkflowName}FinishedMessage(string id, {WorkflowClassName}Context context, bool isSuccess, string? message = null)
    {{
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        IsSuccess = isSuccess;
        Message = message;
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程失败消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
[WorkflowStage({WorkflowName}Stages.Failure)]
public sealed partial class {WorkflowName}FailureMessage : I{WorkflowName}FailureMessage, I{WorkflowName}ContextCarrier, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - <see cref=""{Names.WorkflowNameStagesClass}.Failure""/>
    /// </summary>
    public const string EventName = {Names.WorkflowNameStagesClass}.Failure;

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc/>
    public string Id {{ get; }}

    /// <inheritdoc/>
    [JsonIgnore]
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public string Message {{ get; }}

    /// <inheritdoc/>
    public string? RemoteStackTrace {{ get; }}

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}FailureMessage""/>
    public {WorkflowName}FailureMessage(string id, {WorkflowClassName}Context context, string message, string? remoteStackTrace)
    {{
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Id = id ?? throw new ArgumentNullException(nameof(id));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Message = message;
        RemoteStackTrace = remoteStackTrace;
    }}
}}");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
[WorkflowStage({WorkflowName}Stages.{stage.Name})]
public sealed partial class Stage{stage.Name}Message : {WorkflowName}StageMessageBase, IStage{stage.Name}, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - <see cref=""{Names.WorkflowNameStagesClass}.{stage.Name}""/>
    /// </summary>
    public const string EventName = {Names.WorkflowNameStagesClass}.{stage.Name};

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc cref=""Stage{stage.Name}Message""/>
    public Stage{stage.Name}Message(string id, {WorkflowClassName}Context context) : base(id, context)
    {{
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的完成消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
[WorkflowStage({WorkflowName}Stages.{stage.Name})]
public sealed partial class Stage{stage.Name}CompletedMessage : {WorkflowName}StageCompletedMessageBase, IStage{stage.Name}, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {{<see cref=""{Names.WorkflowNameStagesClass}.{stage.Name}""/>}}.Completed
    /// </summary>
    public const string EventName = $""{{{Names.WorkflowNameStagesClass}.{stage.Name}}}.Completed"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc cref=""Stage{stage.Name}CompletedMessage""/>
    public Stage{stage.Name}CompletedMessage(string id, {WorkflowClassName}Context context) : base(id, context)
    {{
    }}
}}
");
        }

        yield return new($"Workflow.{WorkflowName}.Messages.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
