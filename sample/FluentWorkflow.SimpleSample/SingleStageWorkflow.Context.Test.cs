#pragma warning disable CS1591

namespace FluentWorkflow.SimpleSample;

public partial class SingleStageTestInfo
{
    #region Public 属性

    public string? ChildWorkflowStartStage { get; set; }

    public int ExceptionStep { get; set; }

    public int Loop { get; set; }

    public int MaxStageDelay { get; set; }

    public int MaxSubWorkflow { get; set; }

    public int Step { get; set; }

    public int StepBase { get; set; }

    /// <summary>
    /// 通过 resume 工作，在每个阶段先挂起再恢复
    /// </summary>
    public bool WorkWithResume { get; set; }

    #endregion Public 属性
}
