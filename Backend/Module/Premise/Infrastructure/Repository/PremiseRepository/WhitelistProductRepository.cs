using Microsoft.EntityFrameworkCore;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Interfaces.Product;
using Premise.Models.Product;
using Shared.Logging;

namespace Premise.Infrastructure.Repository.PremiseRepository
{
    public class WhitelistProductRepository(
        PremiseDbContext context,
        ILogger<WhitelistProductRepository> logger,
        ILogPool logPool) : IWhitelistProductRepository
    {
        private const string Module = "Premise";
        private const string Layer = "Infrastructures/Repository/WhitelistProductRepository";

        private readonly PremiseDbContext _db = context;
        private readonly ILogPool _logPool = logPool;
        private readonly ILogger<WhitelistProductRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<WhitelistProductModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all whitelist products.");
            return await _db.WhitelistProducts.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<WhitelistProductModel?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading whitelist product by id.");
            return await _db.WhitelistProducts.AsNoTracking()
                .FirstOrDefaultAsync(product => product.WhitelistProductId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<WhitelistProductModel>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading whitelist products by ids.");
            var productIds = ids.Distinct().ToArray();
            return await _db.WhitelistProducts.AsNoTracking()
                .Where(product => productIds.Contains(product.WhitelistProductId))
                .ToListAsync(cancellationToken);
        }

        public async Task<WhitelistProductModel?> GetTrackedByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked whitelist product by id.");
            return await _db.WhitelistProducts
                .FirstOrDefaultAsync(product => product.WhitelistProductId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking whitelist product existence.");
            return await _db.WhitelistProducts
                .AnyAsync(product => product.WhitelistProductId == id, cancellationToken);
        }

        public async Task<WhitelistProductModel?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading whitelist product by name.");
            return await _db.WhitelistProducts.AsNoTracking()
                .FirstOrDefaultAsync(product => product.WhitelistProductName == name, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(
            WhitelistProductModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing whitelist product creation.");
            Validate(entity);

            var nameExists = await _db.WhitelistProducts.AnyAsync(
                product => product.WhitelistProductName == entity.WhitelistProductName,
                cancellationToken);
            if (nameExists)
            {
                var exception = new InvalidOperationException(
                    $"Whitelist product already exists: {entity.WhitelistProductName}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Whitelist product creation rejected because its name already exists.");
                throw exception;
            }

            await _db.WhitelistProducts.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Whitelist product staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<WhitelistProductModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing whitelist product range creation.");
            var products = entities.ToList();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var product in products)
            {
                Validate(product);
                if (!names.Add(product.WhitelistProductName) ||
                    await _db.WhitelistProducts.AnyAsync(
                        existing => existing.WhitelistProductName == product.WhitelistProductName,
                        cancellationToken))
                {
                    var exception = new InvalidOperationException(
                        $"Whitelist product already exists: {product.WhitelistProductName}.");
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Whitelist product range creation rejected because a name already exists.");
                    throw exception;
                }
            }

            await _db.WhitelistProducts.AddRangeAsync(products, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Whitelist product range staged for creation.");
        }

        public async Task UpdateAsync(
            WhitelistProductModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing whitelist product update.");
            Validate(entity);
            if (entity.WhitelistProductId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "WhitelistProductModel id is required.", nameof(entity.WhitelistProductId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Whitelist product update rejected because its id is missing.");
                throw exception;
            }

            var exists = await _db.WhitelistProducts.AnyAsync(
                product => product.WhitelistProductId == entity.WhitelistProductId,
                cancellationToken);
            if (!exists)
            {
                var exception = new InvalidOperationException(
                    $"WhitelistProductModel not found: {entity.WhitelistProductId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Whitelist product update rejected because the product was not found.");
                throw exception;
            }

            var nameExists = await _db.WhitelistProducts.AnyAsync(
                product => product.WhitelistProductId != entity.WhitelistProductId &&
                          product.WhitelistProductName == entity.WhitelistProductName,
                cancellationToken);
            if (nameExists)
            {
                var exception = new InvalidOperationException(
                    $"Whitelist product already exists: {entity.WhitelistProductName}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Whitelist product update rejected because its name is already in use.");
                throw exception;
            }

            _db.WhitelistProducts.Update(entity);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Whitelist product staged for update.");
        }

        public void UpdateRange(
            IEnumerable<WhitelistProductModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing whitelist product range update.");
            var products = entities.ToList();
            foreach (var product in products)
            {
                Validate(product);
                if (product.WhitelistProductId == Guid.Empty)
                {
                    var exception = new ArgumentException(
                        "WhitelistProductModel id is required.", nameof(entities));
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Whitelist product range update rejected because an id is missing.");
                    throw exception;
                }
            }

            _db.WhitelistProducts.UpdateRange(products);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Whitelist product range staged for update.");
        }

        #endregion

        private void Validate(WhitelistProductModel entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (!string.IsNullOrWhiteSpace(entity.WhitelistProductName))
            {
                return;
            }

            var exception = new ArgumentException(
                "WhitelistProductModel name is required.", nameof(entity.WhitelistProductName));
            _logger.LogLayerError(_logPool, Module, Layer, exception,
                "Whitelist product rejected because its name is missing.");
            throw exception;
        }
    }
}
