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

    private ImmutableDictionary<string, string> _rawValues;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public string Stage { get; }

    /// <summary>
    /// 原始值
    /// </summary>
    public IReadOnlyDictionary<string, string> Values => _rawValues;

    /// <summary>
    /// 工作流程名称
    /// </summary>
    public string WorkflowName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WorkflowContextMetadata"/>
    public WorkflowContextMetadata(IEnumerable<KeyValuePair<string, string>> values)
    {
        _rawValues = values.ToImmutableDictionary();

        WorkflowName = GetRequiredKey(FluentWorkflowConstants.ContextKeys.WorkflowName);
        Id = GetRequiredKey(FluentWorkflowConstants.ContextKeys.Id);
        Stage = GetRequiredKey(FluentWorkflowConstants.ContextKeys.Stage);
    }

    #endregion Public 构造函数

    #region Private 方法

    private string GetRequiredKey(string key)
    {
        if (Values.TryGetValue(key, out var value)
            && !string.IsNullOrEmpty(value))
        {
            return value;
        }
        throw new InvalidOperationException($"Not found require \"{key}\"");
    }

    #endregion Private 方法

    #region IWorkflowContext

    WorkflowFlag IWorkflowContext.Flag
    {
        get => Values.TryGetValue(FluentWorkflowConstants.ContextKeys.WorkflowFlag, out var valueString) ? Enum.Parse<WorkflowFlag>(valueString) : WorkflowFlag.None;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        set => throw new InvalidOperationException();
    }

    WorkflowContextMetadata? IWorkflowContext.Parent => Values.TryGetValue(FluentWorkflowConstants.ContextKeys.ParentWorkflow, out var valueString) ? IObjectSerializer.Default.Deserialize<WorkflowContextMetadata?>(Convert.FromBase64String(valueString)) : null;

    IReadOnlyDictionary<string, string> IWorkflowContext.GetSnapshot() => Values;

    string? IWorkflowContext.GetValue(string key) => Values.TryGetValue(key, out var value) ? value : default;

    void IWorkflowContext.SetCurrentStage(string stage) => throw new InvalidOperationException();

    void IWorkflowContext.SetParent(WorkflowContextMetadata parent) => throw new InvalidOperationException();

    void IWorkflowContext.SetValue(string key, string? value)
    {
        //如果未修改值，直接返回
        if (_rawValues.TryGetValue(key, out var existedValue))
        {
            if (string.Equals(existedValue, value, StringComparison.Ordinal))
            {
                return;
            }
        }

        if (FluentWorkflowConstants.ContextKeys.IsInitOnlyKey(key))
        {
            throw new InvalidOperationException();
        }

        if (value is null)
        {
            _rawValues = _rawValues.Remove(key);
        }
        else
        {
            _rawValues = _rawValues.SetItem(key, value);
        }
    }

    #endregion IWorkflowContext

    #region IKeyValuesConvertable

    /// <inheritdoc/>
    public static WorkflowContextMetadata ConstructFromKeyValues(IEnumerable<KeyValuePair<string, string>> values) => new(values);

    /// <inheritdoc/>
    public static IEnumerable<KeyValuePair<string, string>> ConvertToKeyValues(WorkflowContextMetadata instance) => instance.Values;

    #endregion IKeyValuesConvertable
}
