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

    public string NameSpace => WorkflowDeclaration.NameSpace;

    public string StageBuilder { get; }

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

        StageBuilder = $"I{WorkflowClassName}StageBuilder";
        BuildStagesMethod = "BuildStages";
        WorkflowNameStagesClass = $"{WorkflowClassName}Stages";

        WorkflowStateMachineDriver = $"{WorkflowClassName}StateMachineDriver";

        StartRequestMessage = $"{WorkflowClassName}StartRequestMessage";

        #region context base

        WorkflowContext = $"{WorkflowClassName}Context";
        StageCompletedMessageInterface = $"I{WorkflowClassName}StageCompletedMessage";
        FailureMessageInterface = $"I{WorkflowClassName}FailureMessage";
        FailureMessage = $"{WorkflowClassName}FailureMessage";

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
