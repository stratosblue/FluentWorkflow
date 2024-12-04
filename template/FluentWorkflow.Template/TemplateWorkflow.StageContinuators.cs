﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Interface;
using Microsoft.Extensions.Logging;
using TemplateNamespace.Handler;

namespace TemplateNamespace.Continuator;

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程延续器
/// </summary>
/// <typeparam name="TStageFinalizer">阶段完成器类型</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateWorkflowContinuator<TStageFinalizer>
    : WorkflowContinuator<TStageFinalizer, ITemplateWorkflow>
    where TStageFinalizer : ITemplateWorkflowStageFinalizer, ITemplateWorkflow
{
    /// <summary>
    /// 阶段完成器
    /// </summary>
    protected readonly TStageFinalizer StageFinalizer;

    /// <inheritdoc cref="TemplateWorkflowContinuator{TStageFinalizer}"/>
    public TemplateWorkflowContinuator(TStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider) : base(workflowAwaitProcessor, logger, serviceProvider)
    {
        StageFinalizer = stageFinalizer ?? throw new ArgumentNullException(nameof(stageFinalizer));
    }

    /// <inheritdoc/>
    protected override Task<TStageFinalizer> GetStageFinalizerAsync(IWorkflowFinishedMessage childWorkflowFinishedMessage, WorkflowContextMetadata parentContextMetadata, CancellationToken cancellationToken)
    {
        return Task.FromResult<TStageFinalizer>(StageFinalizer);
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage1CAUK"/> 的流程延续器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateWorkflowStage1CAUKContinuatorBase
    : TemplateWorkflowContinuator<ITemplateWorkflowStage1CAUKStageFinalizer>
    , IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{
    /// <inheritdoc/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <inheritdoc/>
    public static string StageName => TemplateWorkflowStages.Stage1CAUK;

    /// <inheritdoc cref="TemplateWorkflowStage1CAUKContinuatorBase"/>
    protected TemplateWorkflowStage1CAUKContinuatorBase(ITemplateWorkflowStage1CAUKStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage2BPTG"/> 的流程延续器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateWorkflowStage2BPTGContinuatorBase
    : TemplateWorkflowContinuator<ITemplateWorkflowStage2BPTGStageFinalizer>
    , IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{
    /// <inheritdoc/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <inheritdoc/>
    public static string StageName => TemplateWorkflowStages.Stage2BPTG;

    /// <inheritdoc cref="TemplateWorkflowStage2BPTGContinuatorBase"/>
    protected TemplateWorkflowStage2BPTGContinuatorBase(ITemplateWorkflowStage2BPTGStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage3AWBN"/> 的流程延续器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateWorkflowStage3AWBNContinuatorBase
    : TemplateWorkflowContinuator<ITemplateWorkflowStage3AWBNStageFinalizer>
    , IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{
    /// <inheritdoc/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <inheritdoc/>
    public static string StageName => TemplateWorkflowStages.Stage3AWBN;

    /// <inheritdoc cref="TemplateWorkflowStage3AWBNContinuatorBase"/>
    protected TemplateWorkflowStage3AWBNContinuatorBase(ITemplateWorkflowStage3AWBNStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage1CAUK"/> 的流程延续器
/// </summary>
public partial class TemplateWorkflowStage1CAUKContinuator : TemplateWorkflowStage1CAUKContinuatorBase
{
    /// <inheritdoc cref="TemplateWorkflowStage1CAUKContinuator"/>
    public TemplateWorkflowStage1CAUKContinuator(ITemplateWorkflowStage1CAUKStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger<TemplateWorkflowStage1CAUKContinuator> logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage2BPTG"/> 的流程延续器
/// </summary>
public partial class TemplateWorkflowStage2BPTGContinuator : TemplateWorkflowStage2BPTGContinuatorBase
{
    /// <inheritdoc cref="TemplateWorkflowStage2BPTGContinuator"/>
    public TemplateWorkflowStage2BPTGContinuator(ITemplateWorkflowStage2BPTGStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger<TemplateWorkflowStage2BPTGContinuator> logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateWorkflowStages.Stage3AWBN"/> 的流程延续器
/// </summary>
public partial class TemplateWorkflowStage3AWBNContinuator : TemplateWorkflowStage3AWBNContinuatorBase
{
    /// <inheritdoc cref="TemplateWorkflowStage3AWBNContinuator"/>
    public TemplateWorkflowStage3AWBNContinuator(ITemplateWorkflowStage3AWBNStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger<TemplateWorkflowStage3AWBNContinuator> logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}
