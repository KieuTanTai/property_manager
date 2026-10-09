using Microsoft.EntityFrameworkCore;
using Shared.Enum;
using Shared.Logging;
using TicketAndNotification.Infrastructure.Persistence.DbContext;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Infrastructure.Repository.NotificationRepository
{
    public class NotificationRepository(
        TicketAndNotificationDbContext context,
        ILogger<NotificationRepository> logger,
        ILogPool logPool) : INotificationRepository
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Infrastructure/Repository/NotificationRepository";

        private readonly TicketAndNotificationDbContext _db = context;
        private readonly ILogger<NotificationRepository> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region POST

        public async Task AddAsync(
            NotificationModel notification,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing notification creation.");
            if (notification.NotificationSenderAccountId == Guid.Empty
                || string.IsNullOrWhiteSpace(notification.NotificationContent))
            {
                var exception = new ArgumentException(
                    "NotificationModel sender account id and content are required.",
                    nameof(notification));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification creation rejected because a required value is missing.");
                throw exception;
            }

            await _db.Notifications.AddAsync(notification, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<NotificationModel> notifications,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing notification range creation.");
            var notificationModels = notifications.ToList();
            await _db.Notifications.AddRangeAsync(notificationModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification range staged for creation.");
        }

        public async Task UpdateAsync(
            NotificationModel notification,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing notification update.");
            if (notification.NotificationId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "NotificationModel id is required.",
                    nameof(notification.NotificationId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification update rejected because the id is missing.");
                throw exception;
            }

            var existingNotification = await _db.Notifications.AsNoTracking()
                .FirstOrDefaultAsync(
                    existing => existing.NotificationId == notification.NotificationId,
                    cancellationToken);
            if (existingNotification is null)
            {
                var exception = new InvalidOperationException(
                    $"NotificationModel not found! {notification.NotificationId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification update rejected because it was not found.");
                throw exception;
            }

            _db.Notifications.Update(notification);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification staged for update.");
        }

        public void UpdateRange(
            IEnumerable<NotificationModel> notifications,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing notification range update.");
            var notificationModels = notifications.ToList();
            _db.Notifications.UpdateRange(notificationModels);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification range staged for update.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<NotificationModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all notifications.");
            return await _db.Notifications.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<NotificationModel?> GetByIdAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading notification by id.");
            return await _db.Notifications.AsNoTracking()
                .FirstOrDefaultAsync(
                    notification => notification.NotificationId == notificationId,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationModel>> GetByIdsAsync(
            IEnumerable<Guid> notificationIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading notifications by ids.");
            return await _db.Notifications.AsNoTracking()
                .Where(notification => notificationIds.Contains(notification.NotificationId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationModel>> GetBySenderAccountIdAsync(
            Guid senderAccountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notifications by sender account id.");
            return await _db.Notifications.AsNoTracking()
                .Where(notification => notification.NotificationSenderAccountId == senderAccountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationModel>> GetByRecipientAccountIdAsync(
            Guid recipientAccountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notifications by recipient account id.");
            return await _db.Notifications.AsNoTracking()
                .Where(notification => notification.NotificationRecipients
                    .Any(recipient => recipient.AccountId == recipientAccountId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationModel>> GetByTypeAsync(
            ENotificationType type,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading notifications by type.");
            return await _db.Notifications.AsNoTracking()
                .Where(notification => notification.NotificationType == type)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationModel>> GetByIsReadAsync(
            bool isRead,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notifications by read status.");
            return await _db.Notifications.AsNoTracking()
                .Where(notification => notification.NotificationIsRead == isRead)
                .ToListAsync(cancellationToken);
        }

        #endregion
    }
}
