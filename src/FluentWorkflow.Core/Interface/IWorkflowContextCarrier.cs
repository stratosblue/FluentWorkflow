namespace FluentWorkflow.Interface;

/// <summary>
/// <typeparamref name="TContext"/> 携带者
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IWorkflowContextCarrier<out TContext>
{
    #region Public 属性

    /// <summary>
    /// <typeparamref name="TContext"/>
    /// </summary>
    TContext Context { get; }

    #endregion Public 属性
}
