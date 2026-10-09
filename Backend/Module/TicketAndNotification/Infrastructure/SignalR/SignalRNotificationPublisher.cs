using Microsoft.AspNetCore.SignalR;
using TicketAndNotification.Interfaces.Services;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Infrastructure.SignalR
{
    public sealed class SignalRNotificationPublisher(
        IHubContext<NotificationHub> hubContext) : INotificationRealtimePublisher
    {
        public Task PublishToRecipientAsync(
            Guid accountId,
            NotificationModel notification,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return hubContext.Clients
                .Group(NotificationHub.GetAccountGroupName(accountId))
                .SendAsync(
                    "ReceiveNotification",
                    new
                    {
                        notification.NotificationId,
                        notification.NotificationContent,
                        notification.NotificationType,
                        notification.NotificationCreatedAt
                    },
                    cancellationToken);
        }
    }
}
