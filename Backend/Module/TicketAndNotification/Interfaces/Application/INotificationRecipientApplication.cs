using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Interfaces.Application
{
    public interface INotificationRecipientApplication
    {
        Task<IReadOnlyList<NotificationRecipientModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationRecipientModel>> GetByNotificationIdAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationRecipientModel>> GetByAccountIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);

        Task<NotificationRecipientModel?> GetByIdAsync(
            Guid notificationId,
            Guid accountId,
            CancellationToken cancellationToken = default);

        Task<NotificationRecipientModel> AddAsync(
            NotificationRecipientModel recipient,
            CancellationToken cancellationToken = default);

        Task<int> AddRangeAsync(
            IEnumerable<NotificationRecipientModel> recipients,
            CancellationToken cancellationToken = default);
    }
}
