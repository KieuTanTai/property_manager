using Premise.Infrastructure.Persistence.DbContext;
using Shared.Interfaces;
using Shared.Logging;

namespace Premise.Infrastructure.Repository
{
    public class EfPremiseUnitOfWork(
        PremiseDbContext context,
        ILogger<EfPremiseUnitOfWork> logger,
        ILogPool logPool) : IUnitOfWork
    {
        private readonly PremiseDbContext _context = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<EfPremiseUnitOfWork> _logger = logger;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, "Premise", "Infrastructures/Repository",
                "Saving Premise unit of work changes.");
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, "Premise", "Infrastructures/Repository",
                "Premise unit of work changes saved.");
            return affectedRows;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
