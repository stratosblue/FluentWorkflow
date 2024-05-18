using System.ComponentModel;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Util;

/// <summary>
/// 工作流程 序列化与恢复 工具类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WorkflowSerializeResumeUtil
{
    #region Public 方法

    /// <summary>
    /// 恢复工作流程的执行
    /// </summary>
    /// <typeparam name="TWorkflow"></typeparam>
    /// <typeparam name="TWorkflowContext"></typeparam>
    /// <param name="serializedContext">已序列化的上下文信息</param>
    /// <param name="serviceProvider">用于执行的 <see cref="IServiceProvider"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="WorkflowInvalidOperationException"></exception>
    public static async Task ResumeAsync<TWorkflow, TWorkflowContext>(byte[] serializedContext, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        where TWorkflow : IWorkflow, IWorkflowContextCarrier<TWorkflowContext>
        where TWorkflowContext : WorkflowContext
    {
        ArgumentNullException.ThrowIfNull(serializedContext);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        await using var serviceScope = serviceProvider.CreateAsyncScope();
        var objectSerializer = serviceScope.ServiceProvider.GetRequiredService<IObjectSerializer>();

        var context = objectSerializer.Deserialize<TWorkflowContext>(serializedContext)
                      ?? throw new WorkflowInvalidOperationException("Can not deserialize the context from input data.");

        var workflowResumer = serviceScope.ServiceProvider.GetRequiredService<IWorkflowResumer<TWorkflow>>();
        await workflowResumer.ResumeAsync(context, cancellationToken);
    }

    /// <summary>
    /// 序列化上下文 <paramref name="context"/> 以用于存储、恢复流程
    /// </summary>
    /// <param name="context"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static byte[] SerializeContext<TWorkflowContext>(TWorkflowContext context, IServiceProvider serviceProvider)
        where TWorkflowContext : WorkflowContext
    {
        return serviceProvider.GetRequiredService<IObjectSerializer>().SerializeToBytes(context);
    }

    #endregion Public 方法
}
