using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Interfaces.Repository
{
    public interface INotificationRecipientRepository
    {
        Task AddAsync(
            NotificationRecipientModel recipient,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<NotificationRecipientModel> recipients,
            CancellationToken cancellationToken = default);

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
    }
}
