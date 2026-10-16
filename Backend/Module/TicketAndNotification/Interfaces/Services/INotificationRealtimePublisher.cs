using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Interfaces.Services
{
    public interface INotificationRealtimePublisher
    {
        Task PublishToRecipientAsync(
            Guid accountId,
            NotificationModel notification,
            CancellationToken cancellationToken = default);
    }
}
