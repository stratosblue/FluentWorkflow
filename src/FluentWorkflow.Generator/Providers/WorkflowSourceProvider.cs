using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal abstract class WorkflowSourceProvider(GenerateContext context)
    : SourceProvider
{
    #region Protected 字段

    protected readonly WorkflowDeclaration WorkflowDeclaration = context.WorkflowDeclaration;

    #endregion Protected 字段

    #region Protected 属性

    protected GenerateContext Context { get; } = context;

    protected GenerateNames Names => Context.GenerateNames;

    protected string NameSpace => WorkflowDeclaration.NameSpace;

    protected string WorkflowClassName => WorkflowDeclaration.WorkflowClassName;

    protected string WorkflowContextName => Names.WorkflowContext!;

    protected string WorkflowName => WorkflowDeclaration.WorkflowName;

    #endregion Protected 属性
}
