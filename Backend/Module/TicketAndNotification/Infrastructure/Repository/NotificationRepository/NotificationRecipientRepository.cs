using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using TicketAndNotification.Infrastructure.Persistence.DbContext;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Infrastructure.Repository.NotificationRepository
{
    public class NotificationRecipientRepository(
        TicketAndNotificationDbContext context,
        ILogger<NotificationRecipientRepository> logger,
        ILogPool logPool) : INotificationRecipientRepository
    {
        private const string Module = "TicketAndNotification";

        private const string Layer = "Infrastructure/Repository/NotificationRepository";

        private readonly TicketAndNotificationDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<NotificationRecipientRepository> _logger = logger;

        #region DELETE

        public async Task DeleteAsync(Guid firstForeignId, Guid secondForeignId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Deleting notification recipient association by notification and account id.");
            if (firstForeignId == Guid.Empty || secondForeignId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Notification id and account id are required.", nameof(firstForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification recipient deletion rejected because an id is missing.");
                throw exception;
            }

            var recipient = await GetByIdsAsync(firstForeignId, secondForeignId, cancellationToken);
            if (recipient is null)
            {
                var exception = new InvalidOperationException(
                    "Notification recipient association was not found.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification recipient association was not found for deletion.");
                throw exception;
            }

            _db.NotificationRecipients.Remove(recipient);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification recipient association staged for deletion.");
        }

        #endregion

        #region POST

        public async Task AddAsync(NotificationRecipientModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing notification recipient association creation.");
            if (entity.NotificationId == Guid.Empty || entity.AccountId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Notification id and account id are required.", nameof(entity));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification recipient creation rejected because an id is missing.");
                throw exception;
            }

            var existingRecipient = await GetByIdsAsync(
                entity.NotificationId, entity.AccountId, cancellationToken);
            if (existingRecipient is not null)
            {
                var exception = new InvalidOperationException(
                    "Notification recipient association already exists.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification recipient creation rejected because the association already exists.");
                throw exception;
            }

            await _db.NotificationRecipients.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification recipient association staged for creation.");
        }

        public async Task AddRangeAsync(List<NotificationRecipientModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing notification recipient association batch creation.");
            if (entities.Any(entity => entity.NotificationId == Guid.Empty || entity.AccountId == Guid.Empty))
            {
                var exception = new ArgumentException(
                    "Every notification recipient must have a notification id and account id.", nameof(entities));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification recipient batch creation rejected because an id is missing.");
                throw exception;
            }

            await _db.NotificationRecipients.AddRangeAsync(entities, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification recipient associations staged for creation.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<NotificationRecipientModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading all notification recipient associations.");
            return await _db.NotificationRecipients.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationRecipientModel>> GetByNotificationIdAsync(
            Guid notificationId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notification recipient associations by notification id.");
            return await _db.NotificationRecipients.AsNoTracking()
                .Where(recipient => recipient.NotificationId == notificationId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationRecipientModel>> GetByAccountIdAsync(
            Guid accountId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notification recipient associations by account id.");
            return await _db.NotificationRecipients.AsNoTracking()
                .Where(recipient => recipient.AccountId == accountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<NotificationRecipientModel?> GetByIdsAsync(
            Guid firstForeignId, Guid secondForeignId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notification recipient association by foreign ids.");
            return await _db.NotificationRecipients.AsNoTracking().FirstOrDefaultAsync(recipient =>
                    recipient.NotificationId == firstForeignId && recipient.AccountId == secondForeignId,
                cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid firstForeignId, Guid secondForeignId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Checking notification recipient association existence.");
            return await _db.NotificationRecipients.AnyAsync(recipient =>
                    recipient.NotificationId == firstForeignId && recipient.AccountId == secondForeignId,
                cancellationToken);
        }

        #endregion
    }
}