namespace FluentWorkflow.Generator.Model;

internal class GenerateNames
{
    #region Public 属性

    #region ContextDeclaration

    public string FailureMessage { get; }

    public string FailureMessageInterface { get; }

    public string StageCompletedMessageInterface { get; }

    public string WorkflowContext { get; }

    #endregion ContextDeclaration

    #region Completion Failure Stage

    public string WorkflowCompletionStageConstantName { get; } = "Completion";

    public string WorkflowFailureStageConstantName { get; } = "Failure";

    #endregion Completion Failure Stage

    public string BuildStagesMethod { get; }

    public string NameSpace => WorkflowDescriptor.NameSpace;

    public string StageBuilder { get; }

    public string StartRequestMessage { get; }

    public WorkflowDescriptor WorkflowDescriptor { get; }

    public string WorkflowName => WorkflowDescriptor.Name;

    public string WorkflowNameStagesClass { get; }

    public string WorkflowStateMachineDriver { get; }

    #endregion Public 属性

    #region Public 构造函数

    public GenerateNames(WorkflowDescriptor workflowDescriptor)
    {
        WorkflowDescriptor = workflowDescriptor;

        StageBuilder = $"I{WorkflowName}StageBuilder";
        BuildStagesMethod = "BuildStages";
        WorkflowNameStagesClass = $"{WorkflowName}Stages";

        WorkflowStateMachineDriver = $"{WorkflowName}StateMachineDriver";

        StartRequestMessage = $"{WorkflowName}StartRequestMessage";

        #region context base

        WorkflowContext = $"{WorkflowName}Context";
        StageCompletedMessageInterface = $"I{WorkflowName}StageCompletedMessage";
        FailureMessageInterface = $"I{WorkflowName}FailureMessage";
        FailureMessage = $"{WorkflowName}FailureMessage";

        #endregion context base
    }

    #endregion Public 构造函数

    #region Public 方法

    #region Name

    public string CompletedMessageName(StageName stage) => $"{stage.FullName}CompletedMessage";

    public string MessageName(StageName stage) => $"{stage.FullName}Message";

    #endregion Name

    #endregion Public 方法
}
