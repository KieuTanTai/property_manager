namespace Shared.Interfaces.Messaging
{
    public interface IRabbitMqMessageHandler
    {
        string QueueName { get; }
        Task HandleAsync(ReadOnlyMemory<byte> body, CancellationToken cancellationToken);
    }
}