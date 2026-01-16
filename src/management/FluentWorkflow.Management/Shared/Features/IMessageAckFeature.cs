using FluentWorkflow.Management.Shared.Messages.Abstractions;

namespace FluentWorkflow.Management.Shared.Features;

/// <summary>
/// 消息应答特征
/// </summary>
public interface IMessageAckFeature
{
    #region Public 方法

    /// <summary>
    /// 接收到应答消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ReceiveAckAsync<T>(AckMessage<T> message, CancellationToken cancellationToken);

    #endregion Public 方法
}
