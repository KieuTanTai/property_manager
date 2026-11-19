using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Messaging.Options;
using Shared.Interfaces.Messaging;

namespace Shared.Messaging
{
    internal sealed class RabbitMqConsumerService(
        IConnection connection,
        IOptions<RabbitMqOptions> options,
        IEnumerable<IRabbitMqMessageHandler> handlers,
        ILogger<RabbitMqConsumerService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var handlersByQueue = handlers.ToDictionary(handler => handler.QueueName);
            foreach (var queue in options.Value.Queues)
            {
                if (!handlersByQueue.TryGetValue(queue.Name, out var handler)) continue;
                var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                await channel.ExchangeDeclareAsync(queue.Exchange, options.Value.Exchanges.Single(x => x.Name == queue.Exchange).Type,
                    true, cancellationToken: stoppingToken);
                await channel.QueueDeclareAsync(queue.Name, queue.Durable, false, false, cancellationToken: stoppingToken);
                foreach (var routingKey in queue.RoutingKeys)
                    await channel.QueueBindAsync(queue.Name, queue.Exchange, routingKey, cancellationToken: stoppingToken);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        await handler.HandleAsync(ea.Body, stoppingToken);
                        await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "RabbitMQ handler failed for queue {QueueName}", queue.Name);
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                    }
                };
                await channel.BasicConsumeAsync(queue.Name, false, consumer, stoppingToken);
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
