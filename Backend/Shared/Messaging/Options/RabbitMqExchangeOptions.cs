namespace Shared.Messaging.Options
{
    public sealed class RabbitMqExchangeOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "topic";
        public bool Durable { get; set; } = true;
    }
}
