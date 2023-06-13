using FluentWorkflow.Generator.Model;
namespace FluentWorkflow.Generator.Providers;

internal abstract class WorkflowSourceProvider : SourceProvider
{
    #region Protected 字段

    protected readonly WorkflowDescriptor WorkflowDescriptor;

    #endregion Protected 字段

    #region Protected 属性

    protected GenerateContext Context { get; }

    protected GenerateNames Names => Context.GenerateNames;

    protected string NameSpace => WorkflowDescriptor.NameSpace;

    protected string WorkflowContextName => Names.WorkflowContext!;

    protected string WorkflowName => WorkflowDescriptor.Name;

    #endregion Protected 属性

    #region Public 构造函数

    public WorkflowSourceProvider(GenerateContext context)
    {
        Context = context;
        WorkflowDescriptor = context.WorkflowDescriptor;
    }

    #endregion Public 构造函数
}
