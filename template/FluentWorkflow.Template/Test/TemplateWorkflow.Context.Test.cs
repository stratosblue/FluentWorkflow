#pragma warning disable CS1591

namespace TemplateNamespace;

public partial class TemplateWorkflowContext
{
    #region Public 属性

    public string? ChildWorkflowStartStage { get => InnerGet(); set => InnerSet(value); }

    public int Depth { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int ExceptionDepth { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int ExceptionStep { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int MaxStageDelay { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int MaxSubWorkflow { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int Step { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int StepBase { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    /// <summary>
    /// 通过 resume 工作，在每个阶段先挂起再恢复
    /// </summary>
    public bool WorkWithResume { get => InnerGetBoolean() ?? false; set => InnerSetBoolean(value); }

    #endregion Public 属性
}
