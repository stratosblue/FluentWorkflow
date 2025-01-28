using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers.Workflow;

internal class StartRequestHandlerSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public StartRequestHandlerSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}

namespace {NameSpace}.Handler;

/// <summary>
/// <see cref=""{WorkflowClassName}""/> 的启动请求处理器
/// </summary>
/// <typeparam name=""TWorkflow"">用以启动的工作流程具体实现（<see cref=""{WorkflowClassName}""/> 或其派生类型）</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class {WorkflowClassName}StartRequestHandler<TWorkflow>
    : WorkflowStartRequestHandler<TWorkflow, {WorkflowClassName}Context, {WorkflowClassName}StartRequestMessage, I{WorkflowClassName}>
    , I{WorkflowClassName}
    where TWorkflow : {WorkflowClassName}
{{
    /// <inheritdoc cref=""IServiceProvider""/>
    public IServiceProvider ServiceProvider {{ get; }}

    /// <inheritdoc cref=""{WorkflowClassName}StartRequestHandler{{TWorkflow}}""/>
    public {WorkflowClassName}StartRequestHandler(IWorkflowBuilder<TWorkflow> workflowBuilder, IWorkflowScheduler<TWorkflow> workflowScheduler, IServiceProvider serviceProvider) : base(workflowBuilder, workflowScheduler)
    {{
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }}
}}
");

        yield return new($"{WorkflowClassName}.StartRequestHandler.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
