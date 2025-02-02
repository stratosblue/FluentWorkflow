using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FluentWorkflow.Scheduler;

namespace FluentWorkflow;

/// <summary>
/// 工作流程上下文
/// </summary>
public abstract class WorkflowContext
    : PropertyMapObject
    , IWorkflowContext
{
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
        get => State.Flag;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        set
        {
            ThrowIfStarted();
            State.Flag = value;
        }
    }

    /// <inheritdoc/>
    public string Id => Metadata.Id;

    /// <inheritdoc/>
    public WorkflowContextMetadata Metadata => InnerGet<WorkflowContextMetadata>(null, FluentWorkflowConstants.ContextKeys.Metadata) ?? throw new WorkflowInvalidOperationException("上下文数据错误：不存在元数据");

    /// <inheritdoc/>
    public WorkflowContextSnapshot? Parent => InnerGet<WorkflowContextSnapshot>(null, FluentWorkflowConstants.ContextKeys.ParentWorkflow);

    /// <inheritdoc/>
    public string Stage => State.Stage;

    /// <inheritdoc/>
    public WorkflowContextState State => InnerGet<WorkflowContextState>(null, FluentWorkflowConstants.ContextKeys.State) ?? throw new WorkflowInvalidOperationException("上下文数据错误：不存在状态数据");

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="WorkflowContext"/>
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="ArgumentException"></exception>
    public WorkflowContext(string id) : base(StringComparer.Ordinal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var metadata = new WorkflowContextMetadata(ValidWorkflowName(), id);
        var state = new WorkflowContextState(string.Empty, WorkflowStageState.Unknown);

        InnerSetWithoutValidation(metadata, FluentWorkflowConstants.ContextKeys.Metadata);
        InnerSetWithoutValidation(state, FluentWorkflowConstants.ContextKeys.State);
    }

    /// <summary>
    /// <inheritdoc cref="WorkflowContext"/>
    /// </summary>
    /// <param name="values"></param>
    public WorkflowContext(IEnumerable<KeyValuePair<string, string>> values) : base(values, StringComparer.Ordinal)
    {
        ArgumentNullException.ThrowIfNull(State);
        var currentWorkflowName = Metadata.WorkflowName;
        if (currentWorkflowName is null
            || !string.Equals(ValidWorkflowName(), currentWorkflowName))
        {
            throw new ArgumentException($"\"{currentWorkflowName}\" 对于 {GetType()} 是无效的工作流程名称, 这通常是因为使用了错误的上下文数据进行构造。");
        }
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public TValue? GetValue<TValue>(string key) => InnerGet<TValue>(default, key);

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCurrentStage(string stage) => State.Stage = CheckBeforeSetCurrentStage(stage);

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetParent(WorkflowContextSnapshot parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        ThrowIfStarted();

        if (DataContainer.ContainsKey(FluentWorkflowConstants.ContextKeys.ParentWorkflow)
            || ObjectContainer.ContainsKey(FluentWorkflowConstants.ContextKeys.ParentWorkflow))
        {
            throw new InvalidOperationException("The context already has parent context now.");
        }

        ObjectContainer[FluentWorkflowConstants.ContextKeys.ParentWorkflow] = parent;
        Flag |= WorkflowFlag.HasParentWorkflow;
    }

    /// <inheritdoc/>
    public void SetValue<TValue>(string key, TValue? value) => InnerSet(value, key);

    /// <inheritdoc/>
    void IWorkflowContext.ApplyChanges(IWorkflowContext snapshotContext)
    {
        var sourceSnapshot = snapshotContext.GetSnapshot();
        foreach (var (key, value) in sourceSnapshot)
        {
            DataContainer[key] = value;
            ObjectContainer.Remove(key);
        }

        foreach (var key in DataContainer.Keys)
        {
            if (!sourceSnapshot.ContainsKey(key))
            {
                DataContainer.Remove(key);
                ObjectContainer.Remove(key);
            }
        }
    }

    /// <inheritdoc/>
    IReadOnlyDictionary<string, string> IWorkflowContext.GetSnapshot() => GetSnapshot();

    #endregion Public 方法

    #region Protected 方法

    /// <inheritdoc/>
    protected internal override sealed bool IsMutable(string key) => !FluentWorkflowConstants.ContextKeys.IsInitOnlyKey(key);

    /// <summary>
    /// 获取 <paramref name="propName"/> 的值，并确保其不为空，否则抛出异常
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="propName"></param>
    /// <returns></returns>
    [return: NotNull]
    protected internal TValue RequiredInnerGet<TValue>([CallerMemberName] string propName = null!)
    {
        var value = InnerGet<TValue>(default, propName);
        if (value == null)
        {
            throw new InvalidOperationException($"Can not get \"{propName}\" as \"{typeof(TValue)}\" in context.");
        }
        return value;
    }

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

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

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
