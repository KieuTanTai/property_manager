using Payment.Infrastructure.Persistence.DbContext;
using Shared.Interfaces;
using Shared.Logging;

namespace Payment.Infrastructure.Repository
{
    public class EfPaymentUnitOfWork(
        PaymentDbContext context,
        ILogger<EfPaymentUnitOfWork> logger,
        ILogPool logPool) : IUnitOfWork
    {
        private readonly PaymentDbContext _context = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<EfPaymentUnitOfWork> _logger = logger;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, "Payment", "Infrastructure/Repository",
                "Saving Payment unit of work changes.");
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, "Payment", "Infrastructure/Repository",
                "Payment unit of work changes saved.");
            return affectedRows;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
