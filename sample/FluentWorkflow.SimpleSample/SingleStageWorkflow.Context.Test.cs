#pragma warning disable CS1591

namespace FluentWorkflow.SimpleSample;

partial class SingleStageWorkflowContext
{
    #region Public 属性

    public string? ChildWorkflowStartStage { get => InnerGet(); set => InnerSet(value); }

    public int Loop { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int MaxSubWorkflow { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int Step { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int StepBase { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int MaxStageDelay { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    public int ExceptionStep { get => InnerGetValue<int>(); set => InnerSetValue(value); }

    #endregion Public 属性
}
