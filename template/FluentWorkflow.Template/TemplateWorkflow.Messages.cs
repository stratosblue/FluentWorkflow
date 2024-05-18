﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Interface;

namespace TemplateNamespace.Message;

/// <summary>
/// <see cref="TemplateWorkflowContext"/> 携带者接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateWorkflowContextCarrier
    : IWorkflowContextCarrier<TemplateWorkflowContext>
    , ITemplateWorkflow
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 启动请求消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateWorkflowStartRequestMessage : IWorkflowStartRequestMessage, ITemplateWorkflowContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程结束消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateWorkflowFinishedMessage : IWorkflowFinishedMessage, ITemplateWorkflowContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateWorkflowStageMessage : IWorkflowStageMessage, ITemplateWorkflowContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段完成消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateWorkflowStageCompletedMessage : IWorkflowStageCompletedMessage, ITemplateWorkflowContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程失败消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateWorkflowFailureMessage : IWorkflowFailureMessage, ITemplateWorkflowContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public abstract partial class TemplateWorkflowStageMessageBase : ITemplateWorkflowStageMessage
{
    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc cref="TemplateWorkflowStageMessageBase"/>
    protected TemplateWorkflowStageMessageBase(TemplateWorkflowContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段完成消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public abstract partial class TemplateWorkflowStageCompletedMessageBase : ITemplateWorkflowStageCompletedMessage
{
    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc cref="TemplateWorkflowStageCompletedMessageBase"/>
    protected TemplateWorkflowStageCompletedMessageBase(TemplateWorkflowContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 启动请求消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
public sealed partial class TemplateWorkflowStartRequestMessage : ITemplateWorkflowStartRequestMessage
{
    /// <inheritdoc cref="TemplateWorkflowBase.WorkflowName"/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.StartRequest
    /// </summary>
    public const string EventName = "TemplateWorkflow.StartRequest";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc cref="TemplateWorkflowStartRequestMessage"/>
    public TemplateWorkflowStartRequestMessage(IEnumerable<KeyValuePair<string, string>> context)
        : this(new TemplateWorkflowContext(context))
    {
    }

    /// <inheritdoc cref="TemplateWorkflowStartRequestMessage"/>
    [System.Text.Json.Serialization.JsonConstructor]
    public TemplateWorkflowStartRequestMessage(TemplateWorkflowContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程结束消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
public sealed partial class TemplateWorkflowFinishedMessage : ITemplateWorkflowFinishedMessage
{
    /// <inheritdoc cref="TemplateWorkflowBase.WorkflowName"/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.Finished
    /// </summary>
    public const string EventName = "TemplateWorkflow.Finished";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc/>
    public bool IsSuccess { get; }

    /// <inheritdoc/>
    public string? Message { get; }

    /// <inheritdoc cref="TemplateWorkflowFinishedMessage"/>
    public TemplateWorkflowFinishedMessage(TemplateWorkflowContext context, bool isSuccess, string? message = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        IsSuccess = isSuccess;
        Message = message;
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程失败消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateWorkflowStages.Failure)]
public sealed partial class TemplateWorkflowFailureMessage : ITemplateWorkflowFailureMessage, ITemplateWorkflowContextCarrier, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.Failure
    /// </summary>
    public const string EventName = "TemplateWorkflow.Failure";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public string Message { get; }

    /// <inheritdoc/>
    public string? RemoteStackTrace { get; }

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc cref="TemplateWorkflowFailureMessage"/>
    public TemplateWorkflowFailureMessage(TemplateWorkflowContext context, string message, string? remoteStackTrace)
    {
        WorkflowException.ThrowIfNullOrWhiteSpace(message);

        Context = context ?? throw new ArgumentNullException(nameof(context));
        Message = message;
        RemoteStackTrace = remoteStackTrace;
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage1CAUK"/> 的消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateWorkflowStages.Stage1CAUK)]
public sealed partial class TemplateWorkflowStage1CAUKStageMessage : TemplateWorkflowStageMessageBase, ITemplateWorkflowStage1CAUKStage, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.Stage1CAUK
    /// </summary>
    public const string EventName = "TemplateWorkflow.Stage1CAUK";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="TemplateWorkflowStage1CAUKStageMessage"/>
    public TemplateWorkflowStage1CAUKStageMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage1CAUK"/> 的完成消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateWorkflowStages.Stage1CAUK)]
public sealed partial class TemplateWorkflowStage1CAUKStageCompletedMessage : TemplateWorkflowStageCompletedMessageBase, ITemplateWorkflowStage1CAUKStage, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.Stage1CAUK.Completed
    /// </summary>
    public const string EventName = "TemplateWorkflow.Stage1CAUK.Completed";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="TemplateWorkflowStage1CAUKStageCompletedMessage"/>
    public TemplateWorkflowStage1CAUKStageCompletedMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage2BPTG"/> 的消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateWorkflowStages.Stage2BPTG)]
public sealed partial class TemplateWorkflowStage2BPTGStageMessage : TemplateWorkflowStageMessageBase, ITemplateWorkflowStage2BPTGStage, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.Stage2BPTG
    /// </summary>
    public const string EventName = "TemplateWorkflow.Stage2BPTG";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="TemplateWorkflowStage2BPTGStageMessage"/>
    public TemplateWorkflowStage2BPTGStageMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage2BPTG"/> 的完成消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateWorkflowStages.Stage2BPTG)]
public sealed partial class TemplateWorkflowStage2BPTGStageCompletedMessage : TemplateWorkflowStageCompletedMessageBase, ITemplateWorkflowStage2BPTGStage, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.Stage2BPTG.Completed
    /// </summary>
    public const string EventName = "TemplateWorkflow.Stage2BPTG.Completed";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="TemplateWorkflowStage2BPTGStageCompletedMessage"/>
    public TemplateWorkflowStage2BPTGStageCompletedMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage3AWBN"/> 的消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateWorkflowStages.Stage3AWBN)]
public sealed partial class TemplateWorkflowStage3AWBNStageMessage : TemplateWorkflowStageMessageBase, ITemplateWorkflowStage3AWBNStage, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.Stage3AWBN
    /// </summary>
    public const string EventName = "TemplateWorkflow.Stage3AWBN";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="TemplateWorkflowStage3AWBNStageMessage"/>
    public TemplateWorkflowStage3AWBNStageMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage3AWBN"/> 的完成消息
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateWorkflowStages.Stage3AWBN)]
public sealed partial class TemplateWorkflowStage3AWBNStageCompletedMessage : TemplateWorkflowStageCompletedMessageBase, ITemplateWorkflowStage3AWBNStage, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - TemplateWorkflow.Stage3AWBN.Completed
    /// </summary>
    public const string EventName = "TemplateWorkflow.Stage3AWBN.Completed";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="TemplateWorkflowStage3AWBNStageCompletedMessage"/>
    public TemplateWorkflowStage3AWBNStageCompletedMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}
