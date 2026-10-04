using Identity.Infrastructure.Persistence.DbContext;
using Shared.Interfaces;
using Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Repository
{
    public sealed class EfIdentityUnitOfWork(
        IdentityDbContext context,
        ILogger<EfIdentityUnitOfWork> logger,
        ILogPool logPool) : IUnitOfWork
    {
        private readonly IdentityDbContext _context = context;
        private readonly ILogger<EfIdentityUnitOfWork> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, "identity", "repository", "Saving Identity unit of work changes.");
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, "identity", "repository", "Identity unit of work changes saved.");
            return affectedRows;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}