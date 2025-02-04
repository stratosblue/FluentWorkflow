using System.ComponentModel;
using FluentWorkflow.Abstractions;
using FluentWorkflow.MessageDispatch;
using Microsoft.Extensions.Options;

namespace FluentWorkflow.RabbitMQ;

/// <summary>
/// RabbitMQ交换机选择器
/// </summary>
public interface IRabbitMQExchangeSelector
{
    #region Public 方法

    /// <summary>
    /// 获取消息 <paramref name="dataTransmissionModel"/> 应该使用的交换机
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="dataTransmissionModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<string> GetExchangeAsync<TMessage>(DataTransmissionModel<TMessage> dataTransmissionModel, CancellationToken cancellationToken = default) where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration;

    #endregion Public 方法
}

/// <summary>
/// 基于 <see cref="RabbitMQOptions"/> 的交换机选择器
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed class OptionsBasedRabbitMQExchangeSelector(IOptionsMonitor<RabbitMQOptions> optionsMonitor) : IRabbitMQExchangeSelector
{
    #region Public 方法

    /// <inheritdoc/>
    public ValueTask<string> GetExchangeAsync<TMessage>(DataTransmissionModel<TMessage> dataTransmissionModel, CancellationToken cancellationToken)
        where TMessage : class, IWorkflowMessage, IWorkflowContextCarrier<IWorkflowContext>, IEventNameDeclaration
    {
        return ValueTask.FromResult(optionsMonitor.CurrentValue.ExchangeName ?? RabbitMQOptions.DefaultExchangeName);
    }

    #endregion Public 方法
}
