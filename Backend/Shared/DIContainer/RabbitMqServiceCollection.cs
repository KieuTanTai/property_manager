using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Interfaces.Messaging;
using Shared.Messaging;
using Shared.Messaging.Options;

namespace Shared.DIContainer
{
    public static class RabbitMqServiceCollection
    {
        public static IServiceCollection AddSharedRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RabbitMqOptions>().Bind(configuration.GetSection(RabbitMqOptions.SectionName))
                .PostConfigure(options => {
                    options.UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
                    options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
                })
                .Validate(options => options.Queues.All(queue => options.Exchanges.Any(exchange => exchange.Name == queue.Exchange)),
                    "Each RabbitMQ queue must reference a configured exchange.")
                .ValidateOnStart();
            services.AddSingleton<IConnection>(serviceProvider => {
                var options = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var factory = new ConnectionFactory
                {
                    HostName = options.HostName,
                    Port = options.Port,
                    UserName = options.UserName,
                    Password = options.Password,
                    VirtualHost = options.VirtualHost,

                    ClientProvidedName = "SharedMessagingClient",
                    AutomaticRecoveryEnabled = true
                };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddHostedService<RabbitMqConsumerService>();
            return services;
        }
    }
}