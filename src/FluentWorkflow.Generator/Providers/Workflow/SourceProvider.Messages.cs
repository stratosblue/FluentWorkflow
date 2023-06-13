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
/// <see cref=""{WorkflowName}Context""/> 携带者接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
public partial interface I{WorkflowName}ContextCarrier
    : IWorkflowContextCarrier<{WorkflowName}Context>
    , I{WorkflowName}
{{
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 启动请求消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
public partial interface I{WorkflowName}StartRequestMessage : IWorkflowStartRequestMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 流程结束消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
public partial interface I{WorkflowName}FinishedMessage : IWorkflowFinishedMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 阶段消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
public partial interface I{WorkflowName}StageMessage : IWorkflowStageMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 阶段完成消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
public partial interface I{WorkflowName}StageCompletedMessage : IWorkflowStageCompletedMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 流程失败消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
public partial interface I{WorkflowName}FailureMessage : IWorkflowFailureMessage, I{WorkflowName}ContextCarrier
{{
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 阶段消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
public abstract partial class {WorkflowName}StageMessageBase : I{WorkflowName}StageMessage
{{
    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public {WorkflowName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}StageMessageBase""/>
    protected {WorkflowName}StageMessageBase({WorkflowName}Context context)
    {{
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 阶段完成消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
public abstract partial class {WorkflowName}StageCompletedMessageBase : I{WorkflowName}StageCompletedMessage
{{
    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public {WorkflowName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}StageCompletedMessageBase""/>
    protected {WorkflowName}StageCompletedMessageBase({WorkflowName}Context context)
    {{
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 启动请求消息
/// </summary>
[WorkflowName({WorkflowName}.WorkflowName)]
public sealed partial class {WorkflowName}StartRequestMessage : I{WorkflowName}StartRequestMessage
{{
    /// <inheritdoc cref=""{WorkflowName}Base.WorkflowName""/>
    public static string WorkflowName => {WorkflowName}.WorkflowName;

    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowName}.StartRequest
    /// </summary>
    public const string EventName = ""{WorkflowName}.StartRequest"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public {WorkflowName}Context Context {{ get; }}

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc cref=""{WorkflowName}StartRequestMessage""/>
    public {WorkflowName}StartRequestMessage(IEnumerable<KeyValuePair<string, string>> context)
        : this(new {WorkflowName}Context(context))
    {{
    }}

    /// <inheritdoc cref=""{WorkflowName}StartRequestMessage""/>
    [System.Text.Json.Serialization.JsonConstructor]
    public {WorkflowName}StartRequestMessage({WorkflowName}Context context)
    {{
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }}
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 流程结束消息
/// </summary>
[WorkflowName({WorkflowName}.WorkflowName)]
public sealed partial class {WorkflowName}FinishedMessage : I{WorkflowName}FinishedMessage
{{
    /// <inheritdoc cref=""{WorkflowName}Base.WorkflowName""/>
    public static string WorkflowName => {WorkflowName}.WorkflowName;

    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowName}.Finished
    /// </summary>
    public const string EventName = ""{WorkflowName}.Finished"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public {WorkflowName}Context Context {{ get; }}

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc/>
    public bool IsSuccess {{ get; }}

    /// <inheritdoc/>
    public string? Message {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}FinishedMessage""/>
    public {WorkflowName}FinishedMessage({WorkflowName}Context context, bool isSuccess, string? message = null)
    {{
        Context = context ?? throw new ArgumentNullException(nameof(context));
        IsSuccess = isSuccess;
        Message = message;
    }}
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 流程失败消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
[WorkflowStage({WorkflowName}Stages.Failure)]
public sealed partial class {WorkflowName}FailureMessage : I{WorkflowName}FailureMessage, I{WorkflowName}ContextCarrier, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowName}.Failure
    /// </summary>
    public const string EventName = ""{WorkflowName}.Failure"";

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
    public {WorkflowName}Context Context {{ get; }}

    /// <inheritdoc cref=""{WorkflowName}FailureMessage""/>
    public {WorkflowName}FailureMessage({WorkflowName}Context context, string message, string? remoteStackTrace)
    {{
        WorkflowException.ThrowIfNullOrWhiteSpace(nameof(message));

        Context = context ?? throw new ArgumentNullException(nameof(context));
        Message = message;
        RemoteStackTrace = remoteStackTrace;
    }}
}}");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
/// <summary>
/// <see cref=""{WorkflowName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
[WorkflowStage({WorkflowName}Stages.{stage.Name})]
public sealed partial class {WorkflowName}{stage.Name}StageMessage : {WorkflowName}StageMessageBase, I{WorkflowName}{stage.Name}Stage, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowName}.{stage.Name}
    /// </summary>
    public const string EventName = ""{WorkflowName}.{stage.Name}"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc cref=""{WorkflowName}{stage.Name}StageMessage""/>
    public {WorkflowName}{stage.Name}StageMessage({WorkflowName}Context context) : base(context)
    {{
    }}
}}

/// <summary>
/// <see cref=""{WorkflowName}""/> 的阶段 <see cref=""{WorkflowName}Stages.{stage.Name}""/> 的完成消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName({WorkflowName}.WorkflowName)]
[WorkflowStage({WorkflowName}Stages.{stage.Name})]
public sealed partial class {WorkflowName}{stage.Name}StageCompletedMessage : {WorkflowName}StageCompletedMessageBase, I{WorkflowName}{stage.Name}Stage, IEventNameDeclaration
{{
    /// <summary>
    /// <inheritdoc cref=""IEventNameDeclaration.EventName""/> - {WorkflowName}.{stage.Name}.Completed
    /// </summary>
    public const string EventName = ""{WorkflowName}.{stage.Name}.Completed"";

    /// <inheritdoc cref=""EventName""/>
    static string IEventNameDeclaration.EventName {{ get; }} = EventName;

    /// <inheritdoc cref=""{WorkflowName}{stage.Name}StageCompletedMessage""/>
    public {WorkflowName}{stage.Name}StageCompletedMessage({WorkflowName}Context context) : base(context)
    {{
    }}
}}
");
        }

        yield return new($"{WorkflowName}.Messages.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
