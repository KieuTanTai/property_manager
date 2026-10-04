using Identity.Infrastructure.Persistence.DbContext;
using Shared.Interfaces;
using Shared.Logging;

namespace Identity.Infrastructure.Repository
{
    public sealed class EfIdentityUnitOfWork(
        IdentityDbContext context,
        ILogger<EfIdentityUnitOfWork> logger,
        ILogPool logPool) : IUnitOfWork
    {
        private readonly IdentityDbContext _context = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<EfIdentityUnitOfWork> _logger = logger;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, "Identity", "Infrastructure/Repository", "Saving Identity unit of work changes.");
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, "Identity", "Infrastructure/Repository", "Identity unit of work changes saved.");
            return affectedRows;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}