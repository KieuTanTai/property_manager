using Microsoft.EntityFrameworkCore;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Interfaces.BusinessType;
using Premise.Models.Business;
using Shared.Logging;

namespace Premise.Infrastructure.Repository.PremiseRepository
{
    public class PremiseBusinessTypeRepository(
        PremiseDbContext context,
        ILogger<PremiseBusinessTypeRepository> logger,
        ILogPool logPool) : IPremiseBusinessTypeRepository
    {
        private const string Module = "Premise";
        private const string Layer = "Infrastructures/Repository/PremiseBusinessTypeRepository";

        private readonly PremiseDbContext _db = context;
        private readonly ILogPool _logPool = logPool;
        private readonly ILogger<PremiseBusinessTypeRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<PremiseBusinessTypeModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all premise business type associations.");
            return await _db.PremiseBusinessTypes.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseBusinessTypeModel>> GetByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading business type associations by premise id.");
            return await _db.PremiseBusinessTypes.AsNoTracking()
                .Where(association => association.PremiseId == premiseId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseBusinessTypeModel>> GetByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading premise associations by business type id.");
            return await _db.PremiseBusinessTypes.AsNoTracking()
                .Where(association => association.BusinessTypeId == businessTypeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseBusinessTypeModel>> GetByPremiseIdsAsync(
            IEnumerable<Guid> premiseIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading associations by premise ids.");
            var ids = premiseIds.Distinct().ToArray();
            return await _db.PremiseBusinessTypes.AsNoTracking()
                .Where(association => ids.Contains(association.PremiseId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseBusinessTypeModel>> GetByBusinessTypeIdsAsync(
            IEnumerable<Guid> businessTypeIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading associations by business type ids.");
            var ids = businessTypeIds.Distinct().ToArray();
            return await _db.PremiseBusinessTypes.AsNoTracking()
                .Where(association => ids.Contains(association.BusinessTypeId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseBusinessTypeModel>> GetTrackedByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading tracked business type associations by premise id.");
            return await _db.PremiseBusinessTypes
                .Where(association => association.PremiseId == premiseId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseBusinessTypeModel>> GetTrackedByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading tracked premise associations by business type id.");
            return await _db.PremiseBusinessTypes
                .Where(association => association.BusinessTypeId == businessTypeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<PremiseBusinessTypeModel?> GetByIdsAsync(
            Guid premiseId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading association by composite id.");
            return await _db.PremiseBusinessTypes.AsNoTracking()
                .FirstOrDefaultAsync(
                    association => association.PremiseId == premiseId &&
                                   association.BusinessTypeId == businessTypeId,
                    cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            Guid premiseId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking premise business type association.");
            return await _db.PremiseBusinessTypes.AnyAsync(
                association => association.PremiseId == premiseId &&
                               association.BusinessTypeId == businessTypeId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(
            PremiseBusinessTypeModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing premise business type association creation.");
            Validate(entity);

            if (await ExistsAsync(entity.PremiseId, entity.BusinessTypeId, cancellationToken))
            {
                var exception = new InvalidOperationException(
                    $"Premise business type association already exists: {entity.PremiseId}, {entity.BusinessTypeId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Association creation rejected because it already exists.");
                throw exception;
            }

            await _db.PremiseBusinessTypes.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Premise business type association staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<PremiseBusinessTypeModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing premise business type association range creation.");
            var associations = entities.ToList();
            var keys = new HashSet<(Guid PremiseId, Guid BusinessTypeId)>();

            foreach (var entity in associations)
            {
                Validate(entity);
                if (!keys.Add((entity.PremiseId, entity.BusinessTypeId)) ||
                    await ExistsAsync(entity.PremiseId, entity.BusinessTypeId, cancellationToken))
                {
                    var exception = new InvalidOperationException(
                        $"Premise business type association already exists: {entity.PremiseId}, {entity.BusinessTypeId}.");
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Association range creation rejected because it contains an existing association.");
                    throw exception;
                }
            }

            await _db.PremiseBusinessTypes.AddRangeAsync(associations, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Premise business type association range staged for creation.");
        }

        public async Task DeleteAsync(
            Guid premiseId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing premise business type association deletion.");
            var association = await _db.PremiseBusinessTypes.FirstOrDefaultAsync(
                item => item.PremiseId == premiseId && item.BusinessTypeId == businessTypeId,
                cancellationToken);
            if (association is null)
            {
                var exception = new InvalidOperationException(
                    $"Premise business type association not found: {premiseId}, {businessTypeId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Association deletion rejected because it was not found.");
                throw exception;
            }

            _db.PremiseBusinessTypes.Remove(association);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Premise business type association staged for deletion.");
        }

        #endregion

        private void Validate(PremiseBusinessTypeModel entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (entity.PremiseId != Guid.Empty && entity.BusinessTypeId != Guid.Empty)
            {
                return;
            }

            var exception = new ArgumentException(
                "Both premise id and business type id are required.", nameof(entity));
            _logger.LogLayerError(_logPool, Module, Layer, exception,
                "Premise business type association rejected because an id is missing.");
            throw exception;
        }
    }
}
