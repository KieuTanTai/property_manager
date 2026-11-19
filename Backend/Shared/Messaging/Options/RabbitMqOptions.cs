namespace Shared.Messaging.Options
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public List<RabbitMqExchangeOptions> Exchanges { get; set; } = [];
        public List<RabbitMqQueueOptions> Queues { get; set; } = [];
    }
}
