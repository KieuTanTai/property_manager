using Microsoft.EntityFrameworkCore;
using Premise.Infrastructures.Persistence.DbContext;
using Premise.Interfaces.BusinessType;
using Premise.Models.Business;
using Shared.Logging;

namespace Premise.Infrastructures.Repository.PremiseRepository
{
    public class BusinessTypeRepository(
        PremiseDbContext context,
        ILogger<BusinessTypeRepository> logger,
        ILogPool logPool) : IBusinessTypeRepository
    {
        private const string Module = "Premise";

        private const string Layer = "Infrastructures/Repository/BusinessTypeRepository";

        private readonly PremiseDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<BusinessTypeRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<BusinessTypeModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all business types.");
            return await _db.BusinessTypes.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<BusinessTypeModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading business type by id.");
            return await _db.BusinessTypes.AsNoTracking()
                .FirstOrDefaultAsync(businessType => businessType.BusinessTypeId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<BusinessTypeModel>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading business types by ids.");
            return await _db.BusinessTypes.AsNoTracking()
                .Where(businessType => ids.Contains(businessType.BusinessTypeId))
                .ToListAsync(cancellationToken);
        }

        public async Task<BusinessTypeModel?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked business type by id.");
            return await _db.BusinessTypes
                .FirstOrDefaultAsync(businessType => businessType.BusinessTypeId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking business type existence.");
            return await _db.BusinessTypes
                .AnyAsync(businessType => businessType.BusinessTypeId == id, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(BusinessTypeModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing business type creation.");
            if (string.IsNullOrWhiteSpace(entity.BusinessTypeName))
            {
                var exception = new ArgumentException("BusinessTypeModel name is required.", nameof(entity.BusinessTypeName));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Business type creation rejected because the name is missing.");
                throw exception;
            }

            var isExisted = await _db.BusinessTypes.AnyAsync(
                businessType => businessType.BusinessTypeName == entity.BusinessTypeName,
                cancellationToken);

            if (isExisted)
            {
                var exception = new InvalidOperationException($"Already existed! \n {entity.BusinessTypeName}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Business type creation rejected because the business type already exists.");
                throw exception;
            }

            await _db.BusinessTypes.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Business type staged for creation.");
        }

        public async Task AddRangeAsync(IEnumerable<BusinessTypeModel> entities, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing business type range update.");
            var businessTypeModels = entities.ToList();
            await _db.BusinessTypes.AddRangeAsync(businessTypeModels, cancellationToken);
        }

        public async Task UpdateAsync(BusinessTypeModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing business type update.");
            if (entity.BusinessTypeId == Guid.Empty)
            {
                var exception = new ArgumentException("BusinessTypeModel id is required.", nameof(entity.BusinessTypeId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Business type update rejected because the id is missing.");
                throw exception;
            }

            var existedBusinessType = await _db.BusinessTypes.AsNoTracking()
                .FirstOrDefaultAsync(
                    businessType => businessType.BusinessTypeId == entity.BusinessTypeId,
                    cancellationToken);

            if (existedBusinessType is null)
            {
                var exception = new InvalidOperationException(
                    $"BusinessTypeModel not found! \n {entity.BusinessTypeId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Business type update rejected because the business type was not found.");
                throw exception;
            }

            var isNameUsed = await _db.BusinessTypes.AnyAsync(
                businessType => businessType.BusinessTypeId != entity.BusinessTypeId && businessType.BusinessTypeName == entity.BusinessTypeName,
                cancellationToken);

            if (isNameUsed)
            {
                var exception = new InvalidOperationException($"Already existed! \n {entity.BusinessTypeName}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Business type update rejected because the name already exists.");
                throw exception;
            }

            _db.BusinessTypes.Update(entity);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Business type staged for update.");
        }
        
        public void UpdateRange(IEnumerable<BusinessTypeModel> entities, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing business type range update.");
            var businessTypeModels = entities.ToList();
            _db.BusinessTypes.UpdateRange(businessTypeModels);
        }

        #endregion
    }
}
