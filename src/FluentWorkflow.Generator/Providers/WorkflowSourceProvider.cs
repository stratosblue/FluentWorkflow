using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal abstract class WorkflowSourceProvider : SourceProvider
{
    #region Protected 字段

    protected readonly WorkflowDeclaration WorkflowDeclaration;

    #endregion Protected 字段

    #region Protected 属性

    protected GenerateContext Context { get; }

    protected GenerateNames Names => Context.GenerateNames;

    protected string NameSpace => WorkflowDeclaration.NameSpace;

    protected string WorkflowClassName => WorkflowDeclaration.WorkflowClassName;

    protected string WorkflowContextName => Names.WorkflowContext!;

    protected string WorkflowName => WorkflowDeclaration.WorkflowName;

    #endregion Protected 属性

    #region Public 构造函数

    public WorkflowSourceProvider(GenerateContext context)
    {
        Context = context;
        WorkflowDeclaration = context.WorkflowDeclaration;
    }

    #endregion Public 构造函数
}
