using Contract.Infrastructure.Persistence.DbContext;
using Shared.Interfaces;
using Shared.Logging;

namespace Contract.Infrastructure.Repository
{
    public class EfContractUnitOfWork(
        ContractDbContext context,
        ILogger<EfContractUnitOfWork> logger,
        ILogPool logPool) : IUnitOfWork
    {
        private readonly ContractDbContext _context = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<EfContractUnitOfWork> _logger = logger;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, "Contract", "Infrastructure/Repository",
                "Saving Contract unit of work changes.");
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, "Contract", "Infrastructure/Repository",
                "Contract unit of work changes saved.");
            return affectedRows;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
