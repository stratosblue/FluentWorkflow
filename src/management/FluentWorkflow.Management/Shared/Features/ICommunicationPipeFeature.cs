namespace FluentWorkflow.Management.Shared.Features;

/// <summary>
/// 通信管道功能
/// </summary>
public interface ICommunicationPipeFeature : IAsyncDisposable
{
    #region Public 方法

    /// <summary>
    /// 发送请求 <paramref name="request"/> 并等待一个类型 <typeparamref name="TRespone"/> 的响应
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRespone"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TRespone?> RequestAsync<TRequest, TRespone>(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// 使用 <paramref name="response"/> 响应id为 <paramref name="ackMessageId"/> 的请求
    /// </summary>
    /// <typeparam name="TRespone"></typeparam>
    /// <param name="ackMessageId"></param>
    /// <param name="response"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ResponeAsync<TRespone>(int ackMessageId, TRespone response, CancellationToken cancellationToken);

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken);

    #endregion Public 方法
}
