namespace Shared.Messaging.Options
{
    public sealed class RabbitMqQueueOptions
    {
        public string Name { get; set; } = string.Empty;
        public bool Durable { get; set; } = true;
        public string Exchange { get; set; } = string.Empty;
        public List<string> RoutingKeys { get; set; } = [];
    }
}
