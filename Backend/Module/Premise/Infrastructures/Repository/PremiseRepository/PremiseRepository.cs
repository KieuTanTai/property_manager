using Microsoft.EntityFrameworkCore;
using Premise.Infrastructures.Persistence.DbContext;
using Premise.Interfaces.IRepository;
using Premise.Models.Premise;
using Shared.Enum;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Premise.Infrastructures.Repository.PremiseRepository
{
    public class PremiseRepository(PremiseDbContext context) : IPremiseRepository
    {
        private readonly PremiseDbContext _db = context;

        #region GET

        public async Task<IReadOnlyList<PremiseModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _db.Premises.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<PremiseModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Premises.AsNoTracking()
                .FirstOrDefaultAsync(premise => premise.PremiseId == id, cancellationToken);
        }

        public async Task<PremiseModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _db.Premises.FirstOrDefaultAsync(premise => premise.PremiseId == id,
                cancellationToken);
        }

        public async Task<PremiseModel> GetPremiseByNameAsync(string premiseName,
            CancellationToken cancellationToken = default)
        {
            return await _db.Premises.AsNoTracking()
                       .FirstOrDefaultAsync(premise => premise.PremiseName == premiseName, cancellationToken)
                   ?? throw new InvalidOperationException("PremiseModel not found!");
        }

        public async Task<PremiseModel> GetTrackedPremiseByNameAsync(string premiseName,
            CancellationToken cancellationToken = default)
        {
            return await _db.Premises.FirstOrDefaultAsync(premise => premise.PremiseName == premiseName,
                       cancellationToken)
                   ?? throw new InvalidOperationException("PremiseModel not found!");
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Premises.AnyAsync(premise => premise.PremiseId == id, cancellationToken);
        }

        public async Task<PremiseModel?> GetPremiseAndNavigationByIdAsync(Guid id, bool isGetLocation = true,
            bool isGetMedia = false, bool isGetBusinessTypes = false, CancellationToken cancellationToken = default)
        {
            var query = _db.Premises.AsNoTracking();

            if (isGetLocation)
            {
                query = query.Include(premise => premise.PremiseLocation);
            }

            if (isGetMedia)
            {
                query = query.Include(premise => premise.PremiseMedia);
            }

            if (isGetBusinessTypes)
            {
                query = query.Include(premise => premise.PremiseBusinessTypes);
            }

            return await query.FirstOrDefaultAsync(premise => premise.PremiseId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseModel>> GetPremisesByFloorAsync(int floor,
            CancellationToken cancellationToken = default)
        {
            return await _db.Premises.AsNoTracking()
                .Where(premise => premise.PremiseFloor == floor)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseModel>> GetPremisesByLocationIdAsync(Guid locationId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Premises.AsNoTracking()
                .Where(premise => premise.PremiseLocationId == locationId)
                .ToListAsync(cancellationToken);
        }

        // Paging methods
        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.Premises.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(premise => premise.PremiseId < cursor.Value);
            }

            query = query.OrderByDescending(premise => premise.PremiseId);
            var premises = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(premises, pageSize, premise => premise.PremiseId,
                cancellationToken);
        }


        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByNameAsync(Guid? cursor, int pageSize, string premiseName,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.Premises.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(premise => premise.PremiseId < cursor.Value);
            }
            query = query.Where(premise => premise.PremiseName.Contains(premiseName));
            query = query.OrderByDescending(premise => premise.PremiseId);
            var premises = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(premises, pageSize, premise => premise.PremiseId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize,
            EPremiseStatus status, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.Premises.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(premise => premise.PremiseId < cursor.Value);
            }

            query = query.Where(premise => premise.PremiseStatus == status);
            query = query.OrderByDescending(premise => premise.PremiseId);
            var premises = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(premises, pageSize, premise => premise.PremiseId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(PremiseModel premiseModel, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(premiseModel.PremiseName))
            {
                throw new ArgumentException("PremiseModel name is required.", nameof(premiseModel.PremiseName));
            }

            var isExisted = await _db.Premises.AnyAsync(
                existedPremise => existedPremise.PremiseName == premiseModel.PremiseName,
                cancellationToken);

            if (isExisted)
            {
                throw new InvalidOperationException($"Already existed! \n {premiseModel.PremiseName}");
            }

            await _db.Premises.AddAsync(premiseModel, cancellationToken);
        }

        public async Task UpdateAsync(PremiseModel premiseModel, CancellationToken cancellationToken = default)
        {
            if (premiseModel.PremiseId == Guid.Empty)
            {
                throw new ArgumentException("PremiseModel id is required.", nameof(premiseModel.PremiseId));
            }

            var existedPremise = await _db.Premises.AsNoTracking()
                .FirstOrDefaultAsync(existed => existed.PremiseId == premiseModel.PremiseId, cancellationToken);

            if (existedPremise is null)
            {
                throw new InvalidOperationException($"PremiseModel not found! \n {premiseModel.PremiseId}");
            }

            _db.Premises.Update(premiseModel);
        }

        #endregion
    }
}