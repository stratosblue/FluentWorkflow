using FluentWorkflow.Abstractions;
using Microsoft.Extensions.Logging;

namespace FluentWorkflow;

public class WorkflowExecuteLogger
{
    #region Private 字段

    private readonly ILogger<WorkflowExecuteLogger> _logger;

    #endregion Private 字段

    #region Public 属性

    public List<StageLog> Stages { get; } = new();

    #endregion Public 属性

    #region Public 构造函数

    public WorkflowExecuteLogger(ILogger<WorkflowExecuteLogger> logger)
    {
        _logger = logger;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Step(string stage, string id)
    {
        lock (Stages)
        {
            Stages.Add(new(stage, id));
        }
        _logger.LogInformation($"Logged Stage: {stage} - {id}");
    }

    public void Step(IWorkflowStageMessage stageMessage) => Step(stageMessage.Stage, stageMessage.Id);

    #endregion Public 方法
}

public record struct StageLog(string Stage, string Id);
