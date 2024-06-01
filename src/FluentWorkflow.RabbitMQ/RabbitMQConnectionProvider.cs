using FluentWorkflow.Util;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FluentWorkflow.RabbitMQ;

internal class RabbitMQConnectionProvider : IRabbitMQConnectionProvider
{
    #region Private 字段

    private readonly IAsyncConnectionFactory _connectionFactory;

    #endregion Private 字段

    #region Public 构造函数

    public RabbitMQConnectionProvider(IOptions<RabbitMQOptions> optionsAccessor)
    {
        if (optionsAccessor is null)
        {
            throw new ArgumentNullException(nameof(optionsAccessor));
        }

        var options = optionsAccessor.Value;
        if (options.ConnectionFactory is not null)
        {
            _connectionFactory = options.ConnectionFactory;
        }
        else
        {
            if (options.Uri is null)
            {
                throw new ArgumentException($"{nameof(RabbitMQOptions)}.{nameof(RabbitMQOptions.Uri)} and {nameof(RabbitMQOptions)}.{nameof(RabbitMQOptions.ConnectionFactory)} can not be null both.");
            }
            _connectionFactory = new ConnectionFactory()
            {
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                Uri = options.Uri,
                ClientProvidedName = $"fwf:{FluentWorkflowEnvironment.Description}-{ObjectTagUtil.GetHashCodeTag(this)}",
            };
        }
    }

    public Task<IConnection> GetAsync(CancellationToken cancellationToken)
    {
        var connection = _connectionFactory.CreateConnection();
        return Task.FromResult(connection);
    }

    #endregion Public 构造函数
}
