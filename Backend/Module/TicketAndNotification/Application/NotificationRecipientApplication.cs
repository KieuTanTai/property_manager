using Shared.Interfaces;
using Shared.Logging;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Interfaces.Services;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Application
{
    public class NotificationRecipientApplication(
        IUnitOfWork unitOfWork,
        IBaseAssociativeRepository<NotificationRecipientModel, Guid> recipientRepository,
        INotificationRepository notificationRepository,
        INotificationRealtimePublisher notificationPublisher,
        ILogger<NotificationRecipientApplication> logger,
        ILogPool logPool) : INotificationRecipientApplication
    {
        private const string Module = "TicketAndNotification";

        private const string Layer = "Application";

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<NotificationRecipientApplication> _logger = logger;

        private readonly INotificationRealtimePublisher _notificationPublisher = notificationPublisher;

        private readonly INotificationRepository _notificationRepository = notificationRepository;

        private readonly IBaseAssociativeRepository<NotificationRecipientModel, Guid> _recipientRepository = recipientRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        #region GET

        public Task<IReadOnlyList<NotificationRecipientModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _recipientRepository.GetAllAsync(cancellationToken);
        }

        public Task<IReadOnlyList<NotificationRecipientModel>> GetByNotificationIdAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            return _recipientRepository.GetByFirstForeignIdAsync(
                notificationId, cancellationToken);
        }

        public Task<IReadOnlyList<NotificationRecipientModel>> GetByAccountIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            return _recipientRepository.GetBySecondForeignIdAsync(accountId, cancellationToken);
        }

        public Task<NotificationRecipientModel?> GetByIdAsync(
            Guid notificationId,
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            return _recipientRepository.GetByIdsAsync(
                notificationId, accountId, cancellationToken);
        }

        #endregion

        #region POST

        public async Task<NotificationRecipientModel> AddAsync(
            NotificationRecipientModel recipient,
            CancellationToken cancellationToken = default)
        {
            ThrowIfInvalidNotificationRecipientModel(recipient);
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Notification recipient creation operation started.");
            await _recipientRepository.AddAsync(recipient, cancellationToken);
            await SaveChangesOrThrowAsync(cancellationToken);
            await PublishNotificationAsync(recipient, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification recipient creation completed.");
            return recipient;
        }

        public async Task<int> AddRangeAsync(
            IEnumerable<NotificationRecipientModel> recipients,
            CancellationToken cancellationToken = default)
        {
            var recipientModels = recipients.ToList();
            if (recipientModels.Count == 0)
            {
                var exception = new ArgumentException(
                    "Notification recipient collection cannot be empty.",
                    nameof(recipients));
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Notification recipient batch creation rejected because the collection is empty.");
                throw exception;
            }
            recipientModels.ForEach(recipientModel => ThrowIfInvalidNotificationRecipientModel(recipientModel));

            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Notification recipient range creation operation started.");
            await _recipientRepository.AddRangeAsync(
                recipientModels, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync(cancellationToken);
            await PublishNotificationsAsync(recipientModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification recipient range creation completed.");
            return affectedRows;
        }

        #endregion

        #region PRIVATE

        private async Task PublishNotificationsAsync(
            IReadOnlyCollection<NotificationRecipientModel> recipients,
            CancellationToken cancellationToken)
        {
            var notificationIds = recipients
                .Select(recipient => recipient.NotificationId)
                .Distinct()
                .ToArray();
            var notifications = await _notificationRepository.GetByIdsAsync(
                notificationIds, cancellationToken);
            var notificationsById = notifications.ToDictionary(notification => notification.NotificationId);

            foreach (var recipient in recipients)
            {
                if (!notificationsById.TryGetValue(
                        recipient.NotificationId, out var notification))
                {
                    var exception = new InvalidOperationException(
                        $"Notification {recipient.NotificationId} was not found after recipient creation.");
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Notification recipient was saved but its realtime notification could not be loaded.");
                    throw exception;
                }

                await _notificationPublisher.PublishToRecipientAsync(
                    recipient.AccountId, notification, cancellationToken);
            }
        }

        private async Task PublishNotificationAsync(
            NotificationRecipientModel recipient,
            CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(
                recipient.NotificationId, cancellationToken);
            if (notification is null)
            {
                var exception = new InvalidOperationException(
                    $"Notification {recipient.NotificationId} was not found after recipient creation.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification recipient was saved but its realtime notification could not be loaded.");
                throw exception;
            }

            await _notificationPublisher.PublishToRecipientAsync(
                recipient.AccountId, notification, cancellationToken);
        }

        private async Task<int> SaveChangesOrThrowAsync(
            CancellationToken cancellationToken)
        {
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException(
                    "Notification recipient persistence returned zero affected rows.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification recipient creation persistence failed.");
                throw exception;
            }

            return affectedRows;
        }

        private static void ThrowIfInvalidNotificationRecipientModel(NotificationRecipientModel recipient)
        {
            ArgumentNullException.ThrowIfNull(recipient);
            if (recipient.NotificationId == Guid.Empty)
            {
                throw new ArgumentException("Notification id is required.", nameof(recipient));
            }
            if (recipient.AccountId == Guid.Empty)
            {
                throw new ArgumentException("Recipient account id is required.", nameof(recipient));
            }
        }

        #endregion
    }
}