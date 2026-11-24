using Shared.Interfaces;
using Shared.Logging;
using TicketAndNotification.Infrastructure.Persistence.DbContext;

namespace TicketAndNotification.Infrastructure.Repository
{
    public class EfTicketAndNotificationUnitOfWork(
        TicketAndNotificationDbContext context,
        ILogger<EfTicketAndNotificationUnitOfWork> logger,
        ILogPool logPool) : IUnitOfWork
    {
        private readonly TicketAndNotificationDbContext _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            logger.LogLayerDebug(logPool, "TicketAndNotification", "Infrastructure/Repository",
                "Saving TicketAndNotification unit of work changes.");

            var affectedRows = await _context.SaveChangesAsync(cancellationToken);

            logger.LogLayerInformation(logPool, "TicketAndNotification", "Infrastructure/Repository",
                $"TicketAndNotification unit of work changes saved. Affected rows: {affectedRows}.");

            return affectedRows;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
