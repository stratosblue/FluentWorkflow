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

    public string NameSpace => WorkflowDeclaration.NameSpace;

    public string StartRequestMessage { get; }

    public string WorkflowClassName => WorkflowDeclaration.WorkflowClassName;

    public WorkflowDeclaration WorkflowDeclaration { get; }

    public string WorkflowName => WorkflowDeclaration.WorkflowName;

    public string WorkflowNameStagesClass { get; }

    public string WorkflowStateMachineDriver { get; }

    #endregion Public 属性

    #region Public 构造函数

    public GenerateNames(WorkflowDeclaration workflowDeclaration)
    {
        WorkflowDeclaration = workflowDeclaration;

        WorkflowNameStagesClass = $"{WorkflowName}Stages";

        WorkflowStateMachineDriver = $"{WorkflowClassName}StateMachineDriver";

        StartRequestMessage = $"{WorkflowName}StartRequestMessage";

        #region context base

        WorkflowContext = $"{WorkflowClassName}Context";
        StageCompletedMessageInterface = $"I{WorkflowName}StageCompletedMessage";
        FailureMessageInterface = $"I{WorkflowName}FailureMessage";
        FailureMessage = $"{WorkflowName}FailureMessage";

        #endregion context base
    }

    #endregion Public 构造函数

    #region Public 方法

    #region Name

    public string CompletedMessageName(StageName stage) => $"Stage{stage.Name}CompletedMessage";

    public string MessageName(StageName stage) => $"Stage{stage.Name}Message";

    #endregion Name

    #endregion Public 方法
}
