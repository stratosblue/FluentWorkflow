using System.ComponentModel;
using System.Runtime.CompilerServices;
using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
/// 工作流程上下文元数据
/// </summary>
public record class WorkflowContextMetadata
    : IUniqueId
{
    #region Public 属性

    /// <inheritdoc/>
    public string Id { get; }

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public string WorkflowName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowContextMetadata"/>
    public WorkflowContextMetadata(string workflowName, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        WorkflowName = workflowName;
        Id = id;
    }

    #endregion Public 构造函数
}

/// <summary>
/// 工作流程上下文状态
/// </summary>
public class WorkflowContextState
    : ICurrentStage
{
    #region Private 字段

    private string? _alias;

    private string _stage;

    private WorkflowStageState _stageState;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 当前流程实例的别名
    /// </summary>
    public virtual string? Alias
    {
        get => _alias;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        set
        {
            if (_alias != null
                && !string.Equals(_alias, value, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"The context already alias as \"{_alias}\".");
            }

            _alias = value;
        }
    }

    /// <summary>
    /// 工作流程标识
    /// </summary>
    public WorkflowFlag Flag { get; [EditorBrowsable(EditorBrowsableState.Advanced)] set; }

    /// <inheritdoc/>
    public virtual string Stage { get => _stage; internal set => _stage = value; }

    /// <summary>
    /// 当前阶段 <see cref="Stage"/> 的状态
    /// </summary>
    public virtual WorkflowStageState StageState { get => _stageState; internal set => _stageState = value; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowContextState"/>
    public WorkflowContextState(string stage, WorkflowStageState stageState)
    {
        ArgumentNullException.ThrowIfNull(stage);

        _stage = stage;
        _stageState = stageState;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 设置 <see cref="StageState"/>
    /// </summary>
    /// <param name="state"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetStageState(WorkflowStageState state)
    {
        StageState = state;
    }

    #endregion Public 方法
}

/// <summary>
/// 不可变的工作流程上下文状态
/// </summary>
public class ImmutableWorkflowContextState : WorkflowContextState
{
    #region Public 属性

    /// <inheritdoc/>
    public override string? Alias { get => base.Alias; set => throw InvalidOperationException(); }

    /// <inheritdoc/>
    public override string Stage { get => base.Stage; internal set => throw InvalidOperationException(); }

    /// <inheritdoc/>
    public override WorkflowStageState StageState { get => base.StageState; internal set => throw InvalidOperationException(); }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc/>
    public ImmutableWorkflowContextState(string stage, WorkflowStageState stageState, string? alias) : base(stage, stageState)
    {
        base.Alias = alias;
    }

    #endregion Public 构造函数

    #region Private 方法

    private static InvalidOperationException InvalidOperationException([CallerMemberName] string? operationName = null)
    {
        return new InvalidOperationException($"Can not invoke \"{operationName}\".");
    }

    #endregion Private 方法
}
