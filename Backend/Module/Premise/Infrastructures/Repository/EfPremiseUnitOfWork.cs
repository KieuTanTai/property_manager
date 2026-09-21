using Premise.Infrastructures.Persistence.DbContext;
using Shared.Interfaces;

namespace Premise.Infrastructures.Repository
{
    public class EfPremiseUnitOfWork(PremiseDbContext context) : IUnitOfWork
    {
        private readonly PremiseDbContext _context = context;

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