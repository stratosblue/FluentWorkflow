using FluentWorkflow.Abstractions;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
/// 工作流程调试运行器
/// </summary>
public interface IWorkflowDebugRunner
{
    #region Public 方法

    /// <summary>
    /// 开始执行消息对应的流程
    /// </summary>
    /// <param name="eventName">消息名称</param>
    /// <param name="transmissionModelRawData">消息数据传输模型的序列化原始数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RunAsync(string eventName, ReadOnlyMemory<byte> transmissionModelRawData, CancellationToken cancellationToken = default);

    /// <summary>
    /// 开始执行消息对应的流程
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="transmissionModel">消息对应的传输模型实例</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RunAsync<TMessage>(DataTransmissionModel<TMessage> transmissionModel, CancellationToken cancellationToken = default) where TMessage : IWorkflowMessage, IEventNameDeclaration;

    #endregion Public 方法
}
