using Shared.Interfaces;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Interfaces.Repository
{
    public interface INotificationRecipientRepository :
        IBaseAssociativeRepository<NotificationRecipientModel, Guid>
    {
        Task<IReadOnlyList<NotificationRecipientModel>> GetByNotificationIdAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationRecipientModel>> GetByAccountIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);
    }
}
