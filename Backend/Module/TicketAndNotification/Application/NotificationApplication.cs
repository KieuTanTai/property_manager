using Shared.Enum;
using Shared.Interfaces;
using Shared.Logging;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Application
{
    public class NotificationApplication(
        IUnitOfWork unitOfWork,
        INotificationRepository notificationRepository,
        ILogger<NotificationApplication> logger,
        ILogPool logPool) : INotificationApplication
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Application";

        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly INotificationRepository _notificationRepository = notificationRepository;
        private readonly ILogger<NotificationApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        public Task<IReadOnlyList<NotificationModel>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            _notificationRepository.GetAllAsync(cancellationToken);

        public Task<NotificationModel?> GetByIdAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default) =>
            _notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        public Task<IReadOnlyList<NotificationModel>> GetByIdsAsync(
            IEnumerable<Guid> notificationIds,
            CancellationToken cancellationToken = default) =>
            _notificationRepository.GetByIdsAsync(notificationIds, cancellationToken);

        public Task<IReadOnlyList<NotificationModel>> GetBySenderAccountIdAsync(
            Guid senderAccountId,
            CancellationToken cancellationToken = default) =>
            _notificationRepository.GetBySenderAccountIdAsync(
                senderAccountId, cancellationToken);

        public Task<IReadOnlyList<NotificationModel>> GetByRecipientAccountIdAsync(
            Guid recipientAccountId,
            CancellationToken cancellationToken = default) =>
            _notificationRepository.GetByRecipientAccountIdAsync(
                recipientAccountId, cancellationToken);

        public Task<IReadOnlyList<NotificationModel>> GetByTypeAsync(
            ENotificationType type,
            CancellationToken cancellationToken = default) =>
            _notificationRepository.GetByTypeAsync(type, cancellationToken);

        public Task<IReadOnlyList<NotificationModel>> GetByIsReadAsync(
            bool isRead,
            CancellationToken cancellationToken = default) =>
            _notificationRepository.GetByIsReadAsync(isRead, cancellationToken);

        #endregion

        #region POST

        public async Task<NotificationModel> AddAsync(
            NotificationModel notification,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Notification creation operation started.");
            await _notificationRepository.AddAsync(notification, cancellationToken);
            await SaveChangesOrThrowAsync("create notification", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification creation completed.");
            return notification;
        }

        public async Task<int> AddRangeAsync(
            IEnumerable<NotificationModel> notifications,
            CancellationToken cancellationToken = default)
        {
            var notificationModels = RequireItems(notifications, nameof(notifications));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Notification range creation operation started.");
            await _notificationRepository.AddRangeAsync(
                notificationModels, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync(
                "create notifications", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification range creation completed.");
            return affectedRows;
        }

        #endregion

        #region UPDATE

        public async Task<NotificationModel> UpdateAsync(
            NotificationModel notification,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Notification update operation started.");
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
            await SaveChangesOrThrowAsync("update notification", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification update completed.");
            return notification;
        }

        public async Task<int> UpdateRangeAsync(
            IEnumerable<NotificationModel> notifications,
            CancellationToken cancellationToken = default)
        {
            var notificationModels = RequireItems(notifications, nameof(notifications));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Notification range update operation started.");
            _notificationRepository.UpdateRange(notificationModels, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync(
                "update notifications", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification range update completed.");
            return affectedRows;
        }

        #endregion

        private async Task<int> SaveChangesOrThrowAsync(
            string operation,
            CancellationToken cancellationToken)
        {
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException(
                    $"Notification persistence returned zero affected rows while attempting to {operation}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    $"Notification operation failed to {operation}.");
                throw exception;
            }

            return affectedRows;
        }

        private List<NotificationModel> RequireItems(
            IEnumerable<NotificationModel> notifications,
            string parameterName)
        {
            var notificationModels = notifications.ToList();
            if (notificationModels.Count == 0)
            {
                var exception = new ArgumentException(
                    "Notification collection cannot be empty.", parameterName);
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Notification batch operation rejected because the collection is empty.");
                throw exception;
            }

            return notificationModels;
        }
    }
}
