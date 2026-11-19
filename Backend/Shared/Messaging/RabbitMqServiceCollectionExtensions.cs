using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Options;
using Shared.Interfaces.Messaging;

namespace Shared.Messaging
{
    public static class RabbitMqServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RabbitMqOptions>().Bind(configuration.GetSection(RabbitMqOptions.SectionName))
                .Validate(options => options.Queues.All(queue => options.Exchanges.Any(exchange => exchange.Name == queue.Exchange)),
                    "Each RabbitMQ queue must reference a configured exchange.")
                .ValidateOnStart();
            services.AddSingleton<IConnection>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var factory = new ConnectionFactory
                {
                    HostName = options.HostName,
                    Port = options.Port,
                    UserName = options.UserName,
                    Password = options.Password,
                    VirtualHost = options.VirtualHost
                };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddHostedService<RabbitMqConsumerService>();
            return services;
        }
    }
}
