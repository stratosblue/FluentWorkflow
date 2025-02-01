﻿// <Auto-Generated/>

using System.ComponentModel;
using FluentWorkflow;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Handler;
using Microsoft.Extensions.Logging;
using TemplateNamespace.Template.Handler;

namespace TemplateNamespace.Template.Continuator;

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程延续器
/// </summary>
/// <typeparam name="TStageFinalizer">阶段完成器类型</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class TemplateContinuator<TStageFinalizer>
    : WorkflowContinuator<TStageFinalizer, ITemplateWorkflow>
    where TStageFinalizer : ITemplateStageFinalizer, ITemplateWorkflow
{
    /// <summary>
    /// 阶段完成器
    /// </summary>
    protected readonly TStageFinalizer StageFinalizer;

    /// <inheritdoc cref="TemplateContinuator{TStageFinalizer}"/>
    public TemplateContinuator(TStageFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider) : base(workflowAwaitProcessor, logger, serviceProvider)
    {
        StageFinalizer = stageFinalizer ?? throw new ArgumentNullException(nameof(stageFinalizer));
    }

    /// <inheritdoc/>
    protected override Task<TStageFinalizer> GetStageFinalizerAsync(IWorkflowFinishedMessage childWorkflowFinishedMessage, WorkflowContextSnapshot parentContextSnapshot, CancellationToken cancellationToken)
    {
        return Task.FromResult<TStageFinalizer>(StageFinalizer);
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage1CAUK"/> 的流程延续器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class StageStage1CAUKContinuatorBase
    : TemplateContinuator<IStageStage1CAUKFinalizer>
    , IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{
    /// <inheritdoc/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <inheritdoc/>
    public static string StageName => TemplateStages.Stage1CAUK;

    /// <inheritdoc cref="StageStage1CAUKContinuatorBase"/>
    protected StageStage1CAUKContinuatorBase(IStageStage1CAUKFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage2BPTG"/> 的流程延续器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class StageStage2BPTGContinuatorBase
    : TemplateContinuator<IStageStage2BPTGFinalizer>
    , IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{
    /// <inheritdoc/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <inheritdoc/>
    public static string StageName => TemplateStages.Stage2BPTG;

    /// <inheritdoc cref="StageStage2BPTGContinuatorBase"/>
    protected StageStage2BPTGContinuatorBase(IStageStage2BPTGFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage3AWBN"/> 的流程延续器基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class StageStage3AWBNContinuatorBase
    : TemplateContinuator<IStageStage3AWBNFinalizer>
    , IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{
    /// <inheritdoc/>
    public static string WorkflowName => TemplateWorkflow.WorkflowName;

    /// <inheritdoc/>
    public static string StageName => TemplateStages.Stage3AWBN;

    /// <inheritdoc cref="StageStage3AWBNContinuatorBase"/>
    protected StageStage3AWBNContinuatorBase(IStageStage3AWBNFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage1CAUK"/> 的流程延续器
/// </summary>
public partial class StageStage1CAUKContinuator : StageStage1CAUKContinuatorBase
{
    /// <inheritdoc cref="StageStage1CAUKContinuator"/>
    public StageStage1CAUKContinuator(IStageStage1CAUKFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger<StageStage1CAUKContinuator> logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage2BPTG"/> 的流程延续器
/// </summary>
public partial class StageStage2BPTGContinuator : StageStage2BPTGContinuatorBase
{
    /// <inheritdoc cref="StageStage2BPTGContinuator"/>
    public StageStage2BPTGContinuator(IStageStage2BPTGFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger<StageStage2BPTGContinuator> logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 的阶段 <see cref="TemplateStages.Stage3AWBN"/> 的流程延续器
/// </summary>
public partial class StageStage3AWBNContinuator : StageStage3AWBNContinuatorBase
{
    /// <inheritdoc cref="StageStage3AWBNContinuator"/>
    public StageStage3AWBNContinuator(IStageStage3AWBNFinalizer stageFinalizer, IWorkflowAwaitProcessor workflowAwaitProcessor, ILogger<StageStage3AWBNContinuator> logger, IServiceProvider serviceProvider)
        : base(stageFinalizer, workflowAwaitProcessor, logger, serviceProvider)
    {
    }
}
