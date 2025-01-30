using System.ComponentModel;
using System.Text.Json.Serialization;
using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
/// 工作流程上下文快照
/// </summary>
[JsonConverter(typeof(KeyValuesConvertableJsonConverter<WorkflowContextSnapshot>))]
public sealed class WorkflowContextSnapshot
    : IWorkflowContext
    , IKeyValuesConvertable<WorkflowContextSnapshot>
{
    #region Private 字段

    private readonly PropertyMapObject _rawValues;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public WorkflowContextMetadata Metadata => GetRequiredKey<WorkflowContextMetadata>(FluentWorkflowConstants.ContextKeys.Metadata);

    /// <inheritdoc/>
    public WorkflowContextState State => GetRequiredKey<ImmutableWorkflowContextState>(FluentWorkflowConstants.ContextKeys.State);

    /// <inheritdoc/>
    public string Id => Metadata.Id;

    /// <inheritdoc/>
    public string Stage => State.Stage;

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public string WorkflowName => Metadata.WorkflowName;

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowContextSnapshot"/>
    public WorkflowContextSnapshot(IEnumerable<KeyValuePair<string, string>> values)
    {
        _rawValues = new(values, StringComparer.Ordinal);
    }

    #endregion Public 构造函数

    #region Private 方法

    private TValue GetRequiredKey<TValue>(string key)
    {
        return _rawValues.InnerGet<TValue>(default, key) ?? throw new InvalidOperationException($"Not found require \"{key}\"");
    }

    #endregion Private 方法

    #region IWorkflowContext

    WorkflowFlag IWorkflowContext.Flag
    {
        get => State.Flag;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        set => throw new InvalidOperationException();
    }

    WorkflowContextSnapshot? IWorkflowContext.Parent => _rawValues.InnerGet<WorkflowContextSnapshot>(null, FluentWorkflowConstants.ContextKeys.ParentWorkflow);

    IReadOnlyDictionary<string, string> IWorkflowContext.GetSnapshot() => _rawValues.GetSnapshot();

    TValue? IWorkflowContext.GetValue<TValue>(string key) where TValue : default => _rawValues.InnerGet<TValue>(default, key);

    void IWorkflowContext.SetCurrentStage(string stage) => throw new InvalidOperationException();

    void IWorkflowContext.SetParent(WorkflowContextSnapshot parent) => throw new InvalidOperationException();

    void IWorkflowContext.SetValue<TValue>(string key, TValue? value) where TValue : default
    {
        if (FluentWorkflowConstants.ContextKeys.IsInitOnlyKey(key))
        {
            throw new InvalidOperationException($"Can not set init only key \"{key}\".");
        }
        _rawValues.InnerSet(value, key);
    }

    void IWorkflowContext.ApplyChanges(IWorkflowContext snapshotContext)
    {
        var sourceSnapshot = snapshotContext.GetSnapshot();
        foreach (var (key, value) in sourceSnapshot)
        {
            _rawValues.DataContainer[key] = value;
            _rawValues.ObjectContainer.Remove(key);
        }

        foreach (var key in _rawValues.DataContainer.Keys)
        {
            if (!sourceSnapshot.ContainsKey(key))
            {
                _rawValues.DataContainer.Remove(key);
                _rawValues.ObjectContainer.Remove(key);
            }
        }
    }

    #endregion IWorkflowContext

    #region IKeyValuesConvertable

    /// <inheritdoc/>
    public static WorkflowContextSnapshot ConstructFromKeyValues(IEnumerable<KeyValuePair<string, string>> values) => new(values);

    /// <inheritdoc/>
    public static IEnumerable<KeyValuePair<string, string>> ConvertToKeyValues(WorkflowContextSnapshot instance) => instance._rawValues.GetSnapshot();

    #endregion IKeyValuesConvertable
}
