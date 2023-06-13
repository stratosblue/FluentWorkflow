using System.ComponentModel;

namespace TemplateNamespace;

/// <summary>
/// <see cref="TemplateWorkflow"/> 阶段构造器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITemplateWorkflowStageBuilder
{
    /// <summary>
    /// 声明流程开始
    /// </summary>
    /// <returns></returns>
    public ITemplateWorkflowflowStageBuilder Begin();
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 流程阶段构造器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITemplateWorkflowflowStageBuilder
{
    /// <summary>
    /// 声明阶段
    /// </summary>
    /// <param name="stageName">阶段名称（需要为有效的 C# 类型名称字符）</param>
    /// <returns></returns>
    public ITemplateWorkflowflowStageBuilder Then(string stageName);

    /// <summary>
    /// 声明阶段全部结束
    /// </summary>
    public void Completion();
}
