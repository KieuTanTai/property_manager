using Microsoft.EntityFrameworkCore;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Interfaces.IRepository.Product;
using Premise.Models.Product;
using Shared.Logging;

namespace Premise.Infrastructure.Repository.PremiseRepository
{
    public class ProductBusinessTypeRepository(
        PremiseDbContext context,
        ILogger<ProductBusinessTypeRepository> logger,
        ILogPool logPool) : IProductBusinessTypeRepository
    {
        private const string Module = "Premise";
        private const string Layer = "Infrastructures/Repository/ProductBusinessTypeRepository";

        private readonly PremiseDbContext _db = context;
        private readonly ILogPool _logPool = logPool;
        private readonly ILogger<ProductBusinessTypeRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<ProductBusinessTypeModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all product business type associations.");
            return await _db.ProductBusinessTypes.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductBusinessTypeModel>> GetByProductIdAsync(
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading business type associations by product id.");
            return await _db.ProductBusinessTypes.AsNoTracking()
                .Where(association => association.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductBusinessTypeModel>> GetByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading product associations by business type id.");
            return await _db.ProductBusinessTypes.AsNoTracking()
                .Where(association => association.BusinessTypeId == businessTypeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductBusinessTypeModel>> GetByProductIdsAsync(
            IEnumerable<Guid> productIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading associations by product ids.");
            var ids = productIds.Distinct().ToArray();
            return await _db.ProductBusinessTypes.AsNoTracking()
                .Where(association => ids.Contains(association.ProductId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductBusinessTypeModel>> GetByBusinessTypeIdsAsync(
            IEnumerable<Guid> businessTypeIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading associations by business type ids.");
            var ids = businessTypeIds.Distinct().ToArray();
            return await _db.ProductBusinessTypes.AsNoTracking()
                .Where(association => ids.Contains(association.BusinessTypeId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductBusinessTypeModel>> GetTrackedByProductIdAsync(
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading tracked business type associations by product id.");
            return await _db.ProductBusinessTypes
                .Where(association => association.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductBusinessTypeModel>> GetTrackedByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading tracked product associations by business type id.");
            return await _db.ProductBusinessTypes
                .Where(association => association.BusinessTypeId == businessTypeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProductBusinessTypeModel?> GetByIdsAsync(
            Guid productId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading association by composite id.");
            return await _db.ProductBusinessTypes.AsNoTracking()
                .FirstOrDefaultAsync(
                    association => association.ProductId == productId &&
                                   association.BusinessTypeId == businessTypeId,
                    cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            Guid productId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking product business type association.");
            return await _db.ProductBusinessTypes.AnyAsync(
                association => association.ProductId == productId &&
                               association.BusinessTypeId == businessTypeId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(
            ProductBusinessTypeModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing product business type association creation.");
            Validate(entity);

            if (await ExistsAsync(entity.ProductId, entity.BusinessTypeId, cancellationToken))
            {
                var exception = new InvalidOperationException(
                    $"Product business type association already exists: {entity.ProductId}, {entity.BusinessTypeId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Association creation rejected because it already exists.");
                throw exception;
            }

            await _db.ProductBusinessTypes.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Product business type association staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<ProductBusinessTypeModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing product business type association range creation.");
            var associations = entities.ToList();
            var keys = new HashSet<(Guid ProductId, Guid BusinessTypeId)>();

            foreach (var entity in associations)
            {
                Validate(entity);
                if (!keys.Add((entity.ProductId, entity.BusinessTypeId)) ||
                    await ExistsAsync(entity.ProductId, entity.BusinessTypeId, cancellationToken))
                {
                    var exception = new InvalidOperationException(
                        $"Product business type association already exists: {entity.ProductId}, {entity.BusinessTypeId}.");
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Association range creation rejected because it contains an existing association.");
                    throw exception;
                }
            }

            await _db.ProductBusinessTypes.AddRangeAsync(associations, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Product business type association range staged for creation.");
        }

        public async Task DeleteAsync(
            Guid productId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing product business type association deletion.");
            var association = await _db.ProductBusinessTypes.FirstOrDefaultAsync(
                item => item.ProductId == productId && item.BusinessTypeId == businessTypeId,
                cancellationToken);
            if (association is null)
            {
                var exception = new InvalidOperationException(
                    $"Product business type association not found: {productId}, {businessTypeId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Association deletion rejected because it was not found.");
                throw exception;
            }

            _db.ProductBusinessTypes.Remove(association);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Product business type association staged for deletion.");
        }

        #endregion

        private void Validate(ProductBusinessTypeModel entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (entity.ProductId != Guid.Empty && entity.BusinessTypeId != Guid.Empty)
            {
                return;
            }

            var exception = new ArgumentException(
                "Both product id and business type id are required.", nameof(entity));
            _logger.LogLayerError(_logPool, Module, Layer, exception,
                "Product business type association rejected because an id is missing.");
            throw exception;
        }
    }
}
