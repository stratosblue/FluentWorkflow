﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Interface;

namespace TemplateNamespace.Template.Message;

/// <summary>
/// <see cref="TemplateWorkflowContext"/> 携带者接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateContextCarrier
    : IWorkflowContextCarrier<TemplateWorkflowContext>
    , ITemplateWorkflow
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 启动请求消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateStartRequestMessage : IWorkflowStartRequestMessage, ITemplateContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程结束消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateFinishedMessage : IWorkflowFinishedMessage, ITemplateContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateStageMessage : IWorkflowStageMessage, ITemplateContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段完成消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateStageCompletedMessage : IWorkflowStageCompletedMessage, ITemplateContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程失败消息接口
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public partial interface ITemplateFailureMessage : IWorkflowFailureMessage, ITemplateContextCarrier
{
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public abstract partial class TemplateStageMessageBase : ITemplateStageMessage
{
    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc cref="TemplateStageMessageBase"/>
    protected TemplateStageMessageBase(TemplateWorkflowContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段完成消息基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[WorkflowName(TemplateWorkflow.WorkflowName)]
public abstract partial class TemplateStageCompletedMessageBase : ITemplateStageCompletedMessage
{
    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public string Stage => Context.Stage;

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc cref="TemplateStageCompletedMessageBase"/>
    protected TemplateStageCompletedMessageBase(TemplateWorkflowContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 启动请求消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
public sealed partial class TemplateStartRequestMessage : ITemplateStartRequestMessage
{
    /// <inheritdoc cref="TemplateWorkflowBase.WorkflowName"/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.StartRequest
    /// </summary>
    public const string EventName = "Template.StartRequest";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc/>
    public string Id => Context.Id;

    /// <inheritdoc/>
    public TemplateWorkflowContext Context { get; }

    /// <inheritdoc/>
    IWorkflowContext IWorkflowContextCarrier<IWorkflowContext>.Context => Context;

    /// <inheritdoc cref="TemplateStartRequestMessage"/>
    public TemplateStartRequestMessage(IEnumerable<KeyValuePair<string, string>> context)
        : this(new TemplateWorkflowContext(context))
    {
    }

    /// <inheritdoc cref="TemplateStartRequestMessage"/>
    [System.Text.Json.Serialization.JsonConstructor]
    public TemplateStartRequestMessage(TemplateWorkflowContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程结束消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
public sealed partial class TemplateFinishedMessage : ITemplateFinishedMessage
{
    /// <inheritdoc cref="TemplateWorkflowBase.WorkflowName"/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.Finished
    /// </summary>
    public const string EventName = "Template.Finished";

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

    /// <inheritdoc cref="TemplateFinishedMessage"/>
    public TemplateFinishedMessage(TemplateWorkflowContext context, bool isSuccess, string? message = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        IsSuccess = isSuccess;
        Message = message;
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程失败消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateStages.Failure)]
public sealed partial class TemplateFailureMessage : ITemplateFailureMessage, ITemplateContextCarrier, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.Failure
    /// </summary>
    public const string EventName = "Template.Failure";

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

    /// <inheritdoc cref="TemplateFailureMessage"/>
    public TemplateFailureMessage(TemplateWorkflowContext context, string message, string? remoteStackTrace)
    {
        WorkflowException.ThrowIfNullOrWhiteSpace(message);

        Context = context ?? throw new ArgumentNullException(nameof(context));
        Message = message;
        RemoteStackTrace = remoteStackTrace;
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage1CAUK"/> 的消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateStages.Stage1CAUK)]
public sealed partial class StageStage1CAUKMessage : TemplateStageMessageBase, IStageStage1CAUK, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.Stage1CAUK
    /// </summary>
    public const string EventName = "Template.Stage1CAUK";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="StageStage1CAUKMessage"/>
    public StageStage1CAUKMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage1CAUK"/> 的完成消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateStages.Stage1CAUK)]
public sealed partial class StageStage1CAUKCompletedMessage : TemplateStageCompletedMessageBase, IStageStage1CAUK, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.Stage1CAUK.Completed
    /// </summary>
    public const string EventName = "Template.Stage1CAUK.Completed";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="StageStage1CAUKCompletedMessage"/>
    public StageStage1CAUKCompletedMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage2BPTG"/> 的消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateStages.Stage2BPTG)]
public sealed partial class StageStage2BPTGMessage : TemplateStageMessageBase, IStageStage2BPTG, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.Stage2BPTG
    /// </summary>
    public const string EventName = "Template.Stage2BPTG";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="StageStage2BPTGMessage"/>
    public StageStage2BPTGMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage2BPTG"/> 的完成消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateStages.Stage2BPTG)]
public sealed partial class StageStage2BPTGCompletedMessage : TemplateStageCompletedMessageBase, IStageStage2BPTG, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.Stage2BPTG.Completed
    /// </summary>
    public const string EventName = "Template.Stage2BPTG.Completed";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="StageStage2BPTGCompletedMessage"/>
    public StageStage2BPTGCompletedMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage3AWBN"/> 的消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateStages.Stage3AWBN)]
public sealed partial class StageStage3AWBNMessage : TemplateStageMessageBase, IStageStage3AWBN, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.Stage3AWBN
    /// </summary>
    public const string EventName = "Template.Stage3AWBN";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="StageStage3AWBNMessage"/>
    public StageStage3AWBNMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage3AWBN"/> 的完成消息
/// </summary>
[WorkflowName(TemplateWorkflow.WorkflowName)]
[WorkflowStage(TemplateStages.Stage3AWBN)]
public sealed partial class StageStage3AWBNCompletedMessage : TemplateStageCompletedMessageBase, IStageStage3AWBN, IEventNameDeclaration
{
    /// <summary>
    /// <inheritdoc cref="IEventNameDeclaration.EventName"/> - Template.Stage3AWBN.Completed
    /// </summary>
    public const string EventName = "Template.Stage3AWBN.Completed";

    /// <inheritdoc cref="EventName"/>
    static string IEventNameDeclaration.EventName { get; } = EventName;

    /// <inheritdoc cref="StageStage3AWBNCompletedMessage"/>
    public StageStage3AWBNCompletedMessage(TemplateWorkflowContext context) : base(context)
    {
    }
}
