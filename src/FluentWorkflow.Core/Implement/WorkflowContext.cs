using System.ComponentModel;
using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
/// 工作流程上下文
/// </summary>
public abstract class WorkflowContext
    : PropertyMapObject
    , IWorkflowContext
{
    #region Private 字段

    private WorkflowFlag? _flag;

    private WorkflowContextMetadata? _parent;

    #endregion Private 字段

    #region Protected 属性

    /// <summary>
    /// 声明工作流程名称
    /// </summary>
    protected abstract string WorkflowName { get; }

    #endregion Protected 属性

    #region Public 属性

    /// <inheritdoc/>
    public WorkflowFlag Flag
    {
        get => _flag ??= this.GetEnum<WorkflowFlag>(FluentWorkflowConstants.ContextKeys.WorkflowFlag, WorkflowFlag.None).Value;
        set
        {
            ThrowIfStarted();
            this.SetEnum<WorkflowFlag>(FluentWorkflowConstants.ContextKeys.WorkflowFlag, value);
        }
    }

    /// <inheritdoc/>
    public string Id => this.Get(FluentWorkflowConstants.ContextKeys.Id) ?? throw new WorkflowInvalidOperationException("上下文数据错误：不存在Id");

    /// <inheritdoc/>
    public WorkflowContextMetadata? Parent => _parent ??= DataContainer.TryGetValue(FluentWorkflowConstants.ContextKeys.ParentWorkflow, out var valueString) ? IObjectSerializer.Default.Deserialize<WorkflowContextMetadata?>(Convert.FromBase64String(valueString)) : null;

    /// <inheritdoc/>
    public string Stage => this.Get(FluentWorkflowConstants.ContextKeys.Stage) ?? throw new WorkflowInvalidOperationException($"上下文数据错误：不存在 {nameof(FluentWorkflowConstants.ContextKeys.Stage)}");

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="WorkflowContext"/>
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="ArgumentException"></exception>
    public WorkflowContext(string id) : base(StringComparer.Ordinal)
    {
        WorkflowException.ThrowIfNullOrWhiteSpace(id);

        this[FluentWorkflowConstants.ContextKeys.Id] = id;
        this[FluentWorkflowConstants.ContextKeys.WorkflowName] = ValidWorkflowName();
        this[FluentWorkflowConstants.ContextKeys.Stage] = string.Empty;
    }

    /// <summary>
    /// <inheritdoc cref="WorkflowContext"/>
    /// </summary>
    /// <param name="values"></param>
    public WorkflowContext(IEnumerable<KeyValuePair<string, string>> values) : base(values, StringComparer.Ordinal)
    {
        if (!DataContainer.TryGetValue(FluentWorkflowConstants.ContextKeys.WorkflowName, out var workflowName)
            || !string.Equals(ValidWorkflowName(), workflowName))
        {
            throw new ArgumentException($"\"{workflowName}\" 对于 {GetType()} 是无效的工作流程名称");
        }

        if (!DataContainer.ContainsKey(FluentWorkflowConstants.ContextKeys.Stage))
        {
            this[FluentWorkflowConstants.ContextKeys.Stage] = string.Empty;
        }
    }

    /// <summary>
    /// <inheritdoc cref="WorkflowContext"/>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="values"></param>
    /// <exception cref="ArgumentException"></exception>
    public WorkflowContext(string id, IEnumerable<KeyValuePair<string, string>> values) : this(values)
    {
        WorkflowException.ThrowIfNullOrWhiteSpace(id);

        this[FluentWorkflowConstants.ContextKeys.Id] = id;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCurrentStage(string stage) => DataContainer[FluentWorkflowConstants.ContextKeys.Stage] = CheckBeforeSetCurrentStage(stage);

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetParent(WorkflowContextMetadata parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        ThrowIfStarted();

        if (DataContainer.ContainsKey(FluentWorkflowConstants.ContextKeys.ParentWorkflow))
        {
            throw new InvalidOperationException("The context already has parent context now.");
        }

        DataContainer[FluentWorkflowConstants.ContextKeys.ParentWorkflow] = Convert.ToBase64String(IObjectSerializer.Default.SerializeToBytes(parent));
        Flag |= WorkflowFlag.HasParentWorkflow;
    }

    /// <inheritdoc/>
    IReadOnlyDictionary<string, string> IWorkflowContext.GetSnapshot() => DataContainer.AsReadOnly();

    /// <inheritdoc/>
    string? IWorkflowContext.GetValue(string key) => this.Get(key);

    /// <inheritdoc/>
    void IWorkflowContext.SetValue(string key, string? value) => this.Set(key, value);

    #endregion Public 方法

    #region Protected 方法

    /// <inheritdoc/>
    protected internal override sealed bool IsMutable(string key) => !FluentWorkflowConstants.ContextKeys.IsInitOnlyKey(key);

    /// <summary>
    /// 在设置阶段前进行检查，并返回进行设置的阶段
    /// </summary>
    /// <param name="stage"></param>
    /// <returns></returns>
    protected abstract string CheckBeforeSetCurrentStage(string stage);

    /// <summary>
    /// 获取有效的工作流程名称
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    protected string ValidWorkflowName()
    {
        var name = WorkflowName;

        WorkflowException.ThrowIfNullOrWhiteSpace(name);

        return name;
    }

    #endregion Protected 方法

    #region Private 方法

    private void ThrowIfStarted()
    {
        if (this.IsStarted())
        {
            throw new InvalidOperationException("The context already started can not change the parent context now.");
        }
    }

    #endregion Private 方法
}
