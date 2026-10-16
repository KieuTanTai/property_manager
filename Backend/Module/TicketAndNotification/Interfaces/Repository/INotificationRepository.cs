using Shared.Enum;
using Shared.Interfaces;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Interfaces.Repository
{
    public interface INotificationRepository : IBaseReadRepository<NotificationModel, Guid>, IBaseUpsertRepository<NotificationModel>
    {

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
