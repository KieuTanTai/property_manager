using Microsoft.EntityFrameworkCore;
using Premise.Infrastructures.Persistence.DbContext;
using Premise.Interfaces.IRepository;
using Premise.Models.Premise;
using Shared.Enum;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Premise.Infrastructures.Repository.PremiseRepository
{
    public class PremiseRepository(
        PremiseDbContext context,
        ILogger<PremiseRepository> logger,
        ILogPool logPool) : IPremiseRepository
    {
        private const string Module = "Premise";

        private const string Layer = "Infrastructures/Repository/PremiseRepository";

        private readonly PremiseDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<PremiseRepository> _logger = logger;

        #region DELETE

        public async Task DeletePremiseBusinessTypesByPremiseIdAsync(Guid premiseId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading premise business type associations by premise id.");
            var premiseBusinessTypes = await _db.PremiseBusinessTypes
                .Where(pbt => pbt.PremiseId == premiseId)
                .ToListAsync(cancellationToken);

            if (!premiseBusinessTypes.Any())
            {
                var exception = new InvalidOperationException($"PremiseBusinessTypes not found! \n {premiseId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise business type deletion rejected because no associations were found.");
                throw exception;
            }

            _db.PremiseBusinessTypes.RemoveRange(premiseBusinessTypes);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Premise business type associations staged for deletion.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<PremiseModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all premises.");
            return await _db.Premises.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<PremiseModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise by id.");
            return await _db.Premises.AsNoTracking()
                .FirstOrDefaultAsync(premise => premise.PremiseId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseModel>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premises by ids.");
            return await _db.Premises.AsNoTracking()
                .Where(premise => ids.Contains(premise.PremiseId))
                .ToListAsync(cancellationToken);
        }

        public async Task<PremiseModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked premise by id.");
            return await _db.Premises.FirstOrDefaultAsync(premise => premise.PremiseId == id,
                cancellationToken);
        }

        public async Task<PremiseModel> GetPremiseByNameAsync(string premiseName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise by name.");
            var premise = await _db.Premises.AsNoTracking()
                .FirstOrDefaultAsync(premise => premise.PremiseName == premiseName, cancellationToken);
            if (premise is null)
            {
                var exception = new InvalidOperationException("PremiseModel not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise lookup rejected because the premise was not found by name.");
                throw exception;
            }

            return premise;
        }

        public async Task<PremiseModel> GetTrackedPremiseByNameAsync(string premiseName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked premise by name.");
            var premise = await _db.Premises.FirstOrDefaultAsync(
                premise => premise.PremiseName == premiseName, cancellationToken);
            if (premise is null)
            {
                var exception = new InvalidOperationException("PremiseModel not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Tracked premise lookup rejected because the premise was not found by name.");
                throw exception;
            }

            return premise;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking premise existence.");
            return await _db.Premises.AnyAsync(premise => premise.PremiseId == id, cancellationToken);
        }

        public async Task<PremiseModel?> GetPremiseAndNavigationByIdAsync(Guid id, bool isGetLocation = true,
            bool isGetMedia = false, bool isGetBusinessTypes = false, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading premise with navigation properties.");
            var query = _db.Premises.AsNoTracking().Where(premise => premise.PremiseId == id);

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

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseModel>> GetPremisesByFloorAsync(int floor,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premises by floor.");
            return await _db.Premises.AsNoTracking()
                .Where(premise => premise.PremiseFloor == floor)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseModel>> GetPremisesByLocationIdAsync(Guid locationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premises by location id.");
            return await _db.Premises.AsNoTracking()
                .Where(premise => premise.PremiseLocationId == locationId)
                .ToListAsync(cancellationToken);
        }

        // Paging methods
        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise page.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise paging rejected because the page size is invalid."));
            var query = _db.Premises.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(premise => premise.PremiseId < cursor.Value);
            }

            query = query.OrderByDescending(premise => premise.PremiseId).Take(pageSize + 1);
            var premises = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(premises, pageSize, premise => premise.PremiseId,
                cancellationToken);
        }


        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByNameAsync(Guid? cursor, int pageSize, string premiseName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise page by name.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise paging rejected because the page size is invalid."));
            var query = _db.Premises.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(premise => premise.PremiseId < cursor.Value);
            }
            query = query.Where(premise => premise.PremiseName.Contains(premiseName));
            query = query.OrderByDescending(premise => premise.PremiseId).Take(pageSize + 1);
            var premises = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(premises, pageSize, premise => premise.PremiseId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize,
            EPremiseStatus status, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise page by status.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise paging rejected because the page size is invalid."));
            var query = _db.Premises.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(premise => premise.PremiseId < cursor.Value);
            }

            query = query.Where(premise => premise.PremiseStatus == status);
            query = query.OrderByDescending(premise => premise.PremiseId).Take(pageSize + 1);
            var premises = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(premises, pageSize, premise => premise.PremiseId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(PremiseModel premiseModel, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing premise creation.");
            if (string.IsNullOrWhiteSpace(premiseModel.PremiseName))
            {
                var exception = new ArgumentException("PremiseModel name is required.", nameof(premiseModel.PremiseName));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise creation rejected because the premise name is missing.");
                throw exception;
            }

            var isExisted = await _db.Premises.AnyAsync(
                existedPremise => existedPremise.PremiseName == premiseModel.PremiseName,
                cancellationToken);

            if (isExisted)
            {
                var exception = new InvalidOperationException($"Already existed! \n {premiseModel.PremiseName}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise creation rejected because the premise already exists.");
                throw exception;
            }

            await _db.Premises.AddAsync(premiseModel, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise staged for creation.");
        }

        public async Task UpdateAsync(PremiseModel premiseModel, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing premise update.");
            if (premiseModel.PremiseId == Guid.Empty)
            {
                var exception = new ArgumentException("PremiseModel id is required.", nameof(premiseModel.PremiseId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise update rejected because the premise id is missing.");
                throw exception;
            }

            var existedPremise = await _db.Premises.AsNoTracking()
                .FirstOrDefaultAsync(existed => existed.PremiseId == premiseModel.PremiseId, cancellationToken);

            if (existedPremise is null)
            {
                var exception = new InvalidOperationException(
                    $"PremiseModel not found! \n {premiseModel.PremiseId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise update rejected because the premise was not found.");
                throw exception;
            }

            _db.Premises.Update(premiseModel);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise staged for update.");
        }

        #endregion
    }
}
