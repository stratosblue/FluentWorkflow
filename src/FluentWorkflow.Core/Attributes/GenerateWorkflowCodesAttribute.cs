#pragma warning disable IDE0060
#pragma warning disable IDE0130
#pragma warning disable IDE0290

namespace FluentWorkflow;

/// <summary>
/// 声明程序集需要生成工作流程定义 <typeparamref name="TDeclaration"/> 的运行支持代码
/// </summary>
/// <typeparam name="TDeclaration"></typeparam>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public sealed class GenerateWorkflowCodesAttribute<TDeclaration> : Attribute where TDeclaration : class
{
    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="GenerateWorkflowCodesAttribute{TDeclaration}"/>
    /// </summary>
    /// <param name="generationMode">生成模式</param>
    public GenerateWorkflowCodesAttribute(WorkflowSourceGenerationMode generationMode = WorkflowSourceGenerationMode.All)
    {
    }

    #endregion Public 构造函数
}
