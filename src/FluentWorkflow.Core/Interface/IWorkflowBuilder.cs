namespace FluentWorkflow.Interface;

/// <summary>
/// <typeparamref name="TWorkflow"/> 构建器
/// </summary>
/// <typeparam name="TWorkflow"></typeparam>
public interface IWorkflowBuilder<out TWorkflow> where TWorkflow : IWorkflow
{
    #region Public 方法

    /// <summary>
    /// 使用上下文原始数据构建 <typeparamref name="TWorkflow"/>
    /// </summary>
    /// <param name="context">上下文数据</param>
    /// <returns></returns>
    TWorkflow Build(IEnumerable<KeyValuePair<string, string>> context);

    /// <summary>
    /// 使用上下文构建 <typeparamref name="TWorkflow"/>
    /// </summary>
    /// <param name="context">上下文数据</param>
    /// <returns></returns>
    TWorkflow Build(IWorkflowContext context);

    #endregion Public 方法
}
