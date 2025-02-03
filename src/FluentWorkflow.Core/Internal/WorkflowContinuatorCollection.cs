using System.Collections;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Handler;

namespace FluentWorkflow;

/// <summary>
/// 工作流程延续器集合
/// </summary>
public class WorkflowContinuatorCollection
    : IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, Type>>>>
{
    #region Private 字段

    private readonly Dictionary<string, Dictionary<string, Type>> _continuators = new(StringComparer.OrdinalIgnoreCase);

    #endregion Private 字段

    #region Public 方法

    /// <summary>
    /// 添加工作流程延续器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Add<T>() where T : IWorkflowContinuator, IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
    {
        if (!_continuators.TryGetValue(T.WorkflowName, out var workflowContinuators))
        {
            workflowContinuators = new(StringComparer.OrdinalIgnoreCase);
            _continuators.Add(T.WorkflowName, workflowContinuators);
        }
        workflowContinuators.Add(T.StageName, typeof(T));
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, IEnumerable<KeyValuePair<string, Type>>>> GetEnumerator()
    {
        foreach (var workflowContinuators in _continuators)
        {
            yield return new(workflowContinuators.Key, workflowContinuators.Value);
        }
    }

    /// <summary>
    /// 移除工作流程延续器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool Remove<T>() where T : IWorkflowContinuator, IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
    {
        if (_continuators.TryGetValue(T.WorkflowName, out var workflowContinuators)
            && workflowContinuators.TryGetValue(T.StageName, out var value)
            && typeof(T).Equals(value)
            && workflowContinuators.Remove(T.StageName))
        {
            if (workflowContinuators.Count == 0)
            {
                _continuators.Remove(T.WorkflowName);
            }
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion Public 方法
}
