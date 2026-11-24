namespace Shared.Messaging.Options
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string VirtualHost { get; set; } = "/";
        public HashSet<RabbitMqExchangeOptions> Exchanges { get; set; } = [];
        public HashSet<RabbitMqQueueOptions> Queues { get; set; } = [];
    }
}