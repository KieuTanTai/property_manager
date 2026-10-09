using Shared.Enum;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Interfaces.Repository
{
    public interface INotificationRepository
    {
        Task AddAsync(
            NotificationModel notification,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<NotificationModel> notifications,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            NotificationModel notification,
            CancellationToken cancellationToken = default);

        void UpdateRange(
            IEnumerable<NotificationModel> notifications,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<NotificationModel?> GetByIdAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationModel>> GetByIdsAsync(
            IEnumerable<Guid> notificationIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationModel>> GetBySenderAccountIdAsync(
            Guid senderAccountId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationModel>> GetByRecipientAccountIdAsync(
            Guid recipientAccountId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationModel>> GetByTypeAsync(
            ENotificationType type,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationModel>> GetByIsReadAsync(
            bool isRead,
            CancellationToken cancellationToken = default);
    }
}
