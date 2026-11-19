using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Options;
using Shared.Interfaces.Messaging;

namespace Shared.Messaging
{
    internal sealed class RabbitMqPublisher(IConnection connection, IOptions<RabbitMqOptions> options) : IRabbitMqPublisher
    {
        public async Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(exchange);
            ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);
            ArgumentNullException.ThrowIfNull(message);

            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            var configuredExchange = options.Value.Exchanges.FirstOrDefault(x => x.Name == exchange)
                                     ?? throw new InvalidOperationException($"RabbitMQ exchange '{exchange}' is not configured.");
            await channel.ExchangeDeclareAsync(configuredExchange.Name, configuredExchange.Type, configuredExchange.Durable,
                cancellationToken: cancellationToken);
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            var properties = new BasicProperties { ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent };
            await channel.BasicPublishAsync(exchange, routingKey, false, properties, body, cancellationToken);
        }
    }
}
