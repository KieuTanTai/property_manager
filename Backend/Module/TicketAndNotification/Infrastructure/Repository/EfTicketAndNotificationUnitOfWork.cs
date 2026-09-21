using Shared.Interfaces;
using TicketAndNotification.Infrastructure.Persistence.DbContext;

namespace TicketAndNotification.Infrastructure.Repository
{
    public class EfTicketAndNotificationUnitOfWork(TicketAndNotificationDbContext context) : IUnitOfWork
    {
        private readonly TicketAndNotificationDbContext _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}