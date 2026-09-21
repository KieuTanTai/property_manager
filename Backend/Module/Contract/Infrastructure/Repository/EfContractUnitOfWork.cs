using Contract.Infrastructure.Persistence.DbContext;
using Shared.Interfaces;

namespace Contract.Infrastructure.Repository
{
    public class EfContractUnitOfWork(ContractDbContext context) : IUnitOfWork
    {
        private readonly ContractDbContext _context = context;

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