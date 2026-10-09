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
        private readonly ILogger<NotificationRecipientRepository> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region POST

        public async Task AddAsync(
            NotificationRecipientModel recipient,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing notification recipient creation.");
            if (recipient.NotificationId == Guid.Empty
                || recipient.AccountId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "NotificationRecipientModel notification and account ids are required.",
                    nameof(recipient));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Notification recipient creation rejected because an id is missing.");
                throw exception;
            }

            await _db.NotificationRecipients.AddAsync(recipient, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification recipient staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<NotificationRecipientModel> recipients,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing notification recipient range creation.");
            var recipientModels = recipients.ToList();
            await _db.NotificationRecipients.AddRangeAsync(recipientModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Notification recipient range staged for creation.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<NotificationRecipientModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading all notification recipients.");
            return await _db.NotificationRecipients.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationRecipientModel>> GetByNotificationIdAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notification recipients by notification id.");
            return await _db.NotificationRecipients.AsNoTracking()
                .Where(recipient => recipient.NotificationId == notificationId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationRecipientModel>> GetByAccountIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notification recipients by account id.");
            return await _db.NotificationRecipients.AsNoTracking()
                .Where(recipient => recipient.AccountId == accountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<NotificationRecipientModel?> GetByIdAsync(
            Guid notificationId,
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading notification recipient by composite id.");
            return await _db.NotificationRecipients.AsNoTracking()
                .FirstOrDefaultAsync(
                    recipient => recipient.NotificationId == notificationId
                                 && recipient.AccountId == accountId,
                    cancellationToken);
        }

        #endregion
    }
}
