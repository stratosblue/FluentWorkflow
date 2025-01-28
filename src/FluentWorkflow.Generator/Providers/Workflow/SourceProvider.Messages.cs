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

namespace {NameSpace}.Message;

/// <summary>
/// <see cref=""{WorkflowClassName}Context""/> 携带者接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowClassName}ContextCarrier
    : IWorkflowContextCarrier<{WorkflowClassName}Context>
    , I{WorkflowClassName}
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 启动请求消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowClassName}StartRequestMessage : IWorkflowStartRequestMessage, I{WorkflowClassName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程结束消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowClassName}FinishedMessage : IWorkflowFinishedMessage, I{WorkflowClassName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 阶段消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowClassName}StageMessage : IWorkflowStageMessage, I{WorkflowClassName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 阶段完成消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowClassName}StageCompletedMessage : IWorkflowStageCompletedMessage, I{WorkflowClassName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程失败消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public partial interface I{WorkflowClassName}FailureMessage : IWorkflowFailureMessage, I{WorkflowClassName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 阶段消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public abstract partial class {WorkflowClassName}StageMessageBase : I{WorkflowClassName}StageMessage
{{
    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowClassName}StageMessageBase""/>
    protected {WorkflowClassName}StageMessageBase({WorkflowClassName}Context context)
    {{
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 阶段完成消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
public abstract partial class {WorkflowClassName}StageCompletedMessageBase : I{WorkflowClassName}StageCompletedMessage
{{
    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowClassName}StageCompletedMessageBase""/>
    protected {WorkflowClassName}StageCompletedMessageBase({WorkflowClassName}Context context)
    {{
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 启动请求消息
/// </summary>
[WorkflowName({WorkflowClassName}.WorkflowName)]
public sealed partial class {WorkflowClassName}StartRequestMessage : I{WorkflowClassName}StartRequestMessage
{{
    /// <inheritdoc cref=""{WorkflowClassName}Base.WorkflowName""/>
    public static string WorkflowName => {WorkflowClassName}.WorkflowName;

    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowClassName}.StartRequest
    /// </summary>
    public const string EventName = ""{WorkflowClassName}.StartRequest"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc cref=""{WorkflowClassName}StartRequestMessage""/>
    public {WorkflowClassName}StartRequestMessage(IEnumerable<KeyValuePair<string, string>> context)
        : this(new {WorkflowClassName}Context(context))
    {{
    }}

    /// <inheritdoc cref=""{WorkflowClassName}StartRequestMessage""/>
    [System.Text.Json.Serialization.JsonConstructor]
    public {WorkflowClassName}StartRequestMessage({WorkflowClassName}Context context)
    {{
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 流程结束消息
/// </summary>
[WorkflowName({WorkflowClassName}.WorkflowName)]
public sealed partial class {WorkflowClassName}FinishedMessage : I{WorkflowClassName}FinishedMessage
{{
    /// <inheritdoc cref=""{WorkflowClassName}Base.WorkflowName""/>
    public static string WorkflowName => {WorkflowClassName}.WorkflowName;

    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowClassName}.Finished
    /// </summary>
    public const string EventName = ""{WorkflowClassName}.Finished"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc/>
    public bool IsSuccess {{ get; }}

    /// <inheritdoc/>
    public string? Message {{ get; }}

    /// <inheritdoc cref=""{WorkflowClassName}FinishedMessage""/>
    public {WorkflowClassName}FinishedMessage({WorkflowClassName}Context context, bool isSuccess, string? message = null)
    {{
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
[WorkflowStage({WorkflowClassName}Stages.Failure)]
public sealed partial class {WorkflowClassName}FailureMessage : I{WorkflowClassName}FailureMessage, I{WorkflowClassName}ContextCarrier, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowClassName}.Failure
    /// </summary>
    public const string EventName = ""{WorkflowClassName}.Failure"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public string Message {{ get; }}

    /// <inheritdoc/>
    public string? RemoteStackTrace {{ get; }}

    /// <inheritdoc/>
    public {WorkflowClassName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowClassName}FailureMessage""/>
    public {WorkflowClassName}FailureMessage({WorkflowClassName}Context context, string message, string? remoteStackTrace)
    {{
        WorkflowException.ThrowIfNullOrWhiteSpace(message);

        Context = context ?? throw new ArgumentNullException(nameof(context));
        Message = message;
        RemoteStackTrace = remoteStackTrace;
    }}
}}");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowClassName}Stages.{stage.Name}""/> 的消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
[WorkflowStage({WorkflowClassName}Stages.{stage.Name})]
public sealed partial class {WorkflowClassName}{stage.Name}StageMessage : {WorkflowClassName}StageMessageBase, I{WorkflowClassName}{stage.Name}Stage, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowClassName}.{stage.Name}
    /// </summary>
    public const string EventName = ""{WorkflowClassName}.{stage.Name}"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc cref=""{WorkflowClassName}{stage.Name}StageMessage""/>
    public {WorkflowClassName}{stage.Name}StageMessage({WorkflowClassName}Context context) : base(context)
    {{
    }}
}}

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的阶段 <see cref=""{WorkflowClassName}Stages.{stage.Name}""/> 的完成消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowClassName}.WorkflowName)]
[WorkflowStage({WorkflowClassName}Stages.{stage.Name})]
public sealed partial class {WorkflowClassName}{stage.Name}StageCompletedMessage : {WorkflowClassName}StageCompletedMessageBase, I{WorkflowClassName}{stage.Name}Stage, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowClassName}.{stage.Name}.Completed
    /// </summary>
    public const string EventName = ""{WorkflowClassName}.{stage.Name}.Completed"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc cref=""{WorkflowClassName}{stage.Name}StageCompletedMessage""/>
    public {WorkflowClassName}{stage.Name}StageCompletedMessage({WorkflowClassName}Context context) : base(context)
    {{
    }}
}}
");
        }

        yield return new($"{WorkflowClassName}.Messages.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
