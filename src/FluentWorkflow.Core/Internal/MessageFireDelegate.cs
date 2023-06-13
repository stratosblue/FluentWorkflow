namespace FluentWorkflow;

/// <summary>
/// 消息触发委托
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <param name="message"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task MessageFireDelegate<in TMessage>(TMessage message, CancellationToken cancellationToken = default);
