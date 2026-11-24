using Microsoft.AspNetCore.SignalR;
using Shared.Logging;
using TicketAndNotification.Interfaces.Services;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Infrastructure.SignalR
{
    public sealed class SignalRNotificationPublisher(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationPublisher> logger,
        ILogPool logPool) : INotificationRealtimePublisher
    {
        private const string Module = "TicketAndNotification";

        private const string Layer = "Infrastructure/SignalR";

        public async Task PublishToRecipientAsync(
            Guid accountId,
            NotificationModel notification,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogLayerDebug(logPool, Module, Layer,
                $"Publishing notification {notification.NotificationId} to account {accountId} over SignalR.");

            try
            {
                await hubContext.Clients
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

                logger.LogLayerInformation(logPool, Module, Layer,
                    $"Notification {notification.NotificationId} published to account {accountId} over SignalR.");
            }
            catch (Exception exception)
            {
                logger.LogLayerError(logPool, Module, Layer, exception,
                    $"Failed to publish notification {notification.NotificationId} to account {accountId} over SignalR.");
                throw;
            }
            finally
            {
                await logPool.FlushAsync(Module, Layer, CancellationToken.None);
            }
        }
    }
}