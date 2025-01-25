using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json.Serialization;
using FluentWorkflow.Interface;

namespace FluentWorkflow;

/// <summary>
/// 工作流程上下文元数据
/// </summary>
[JsonConverter(typeof(KeyValuesConvertableJsonConverter<WorkflowContextMetadata>))]
public sealed class WorkflowContextMetadata
    : IWorkflowContext
    , IKeyValuesConvertable<WorkflowContextMetadata>
{
    #region Private 字段

    private PropertyMapObject _rawValues;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public string Stage { get; }

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public string WorkflowName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowContextMetadata"/>
    public WorkflowContextMetadata(IEnumerable<KeyValuePair<string, string>> values)
    {
        _rawValues = new(values, StringComparer.Ordinal);

        WorkflowName = GetRequiredKey<string>(FluentWorkflowConstants.ContextKeys.WorkflowName);
        Id = GetRequiredKey<string>(FluentWorkflowConstants.ContextKeys.Id);
        Stage = GetRequiredKey<string>(FluentWorkflowConstants.ContextKeys.Stage);
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
        get => _rawValues.InnerGet<WorkflowFlag>(WorkflowFlag.None, FluentWorkflowConstants.ContextKeys.WorkflowFlag);
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        set => throw new InvalidOperationException();
    }

    WorkflowContextMetadata? IWorkflowContext.Parent => _rawValues.InnerGet<WorkflowContextMetadata>(null, FluentWorkflowConstants.ContextKeys.ParentWorkflow);

    IReadOnlyDictionary<string, string> IWorkflowContext.GetSnapshot() => _rawValues.GetSnapshot();

    TValue? IWorkflowContext.GetValue<TValue>(string key) where TValue : default => _rawValues.InnerGet<TValue>(default, key);

    void IWorkflowContext.SetCurrentStage(string stage) => throw new InvalidOperationException();

    void IWorkflowContext.SetParent(WorkflowContextMetadata parent) => throw new InvalidOperationException();

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
    public static WorkflowContextMetadata ConstructFromKeyValues(IEnumerable<KeyValuePair<string, string>> values) => new(values);

    /// <inheritdoc/>
    public static IEnumerable<KeyValuePair<string, string>> ConvertToKeyValues(WorkflowContextMetadata instance) => instance._rawValues.GetSnapshot();

    #endregion IKeyValuesConvertable
}
