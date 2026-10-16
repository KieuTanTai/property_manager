using Microsoft.EntityFrameworkCore;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Interfaces.Premise;
using Premise.Models.Premise;
using Shared.Logging;

namespace Premise.Infrastructure.Repository.PremiseRepository
{
    public class PremiseMediaRepository(
        PremiseDbContext context,
        ILogger<PremiseMediaRepository> logger,
        ILogPool logPool) : IPremiseMediaRepository
    {
        private const string Module = "Premise";
        private const string Layer = "Infrastructures/Repository/PremiseMediaRepository";

        private readonly PremiseDbContext _db = context;
        private readonly ILogPool _logPool = logPool;
        private readonly ILogger<PremiseMediaRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<PremiseMediaModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all premise media.");
            return await _db.PremiseMedias.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<PremiseMediaModel?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise media by id.");
            return await _db.PremiseMedias.AsNoTracking()
                .FirstOrDefaultAsync(media => media.PremiseMediaId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseMediaModel>> GetByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise media by ids.");
            var mediaIds = ids.Distinct().ToArray();
            return await _db.PremiseMedias.AsNoTracking()
                .Where(media => mediaIds.Contains(media.PremiseMediaId))
                .ToListAsync(cancellationToken);
        }

        public async Task<PremiseMediaModel?> GetTrackedByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked premise media by id.");
            return await _db.PremiseMedias
                .FirstOrDefaultAsync(media => media.PremiseMediaId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking premise media existence.");
            return await _db.PremiseMedias
                .AnyAsync(media => media.PremiseMediaId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseMediaModel>> GetByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise media by premise id.");
            return await _db.PremiseMedias.AsNoTracking()
                .Where(media => media.PremiseMediaPremiseId == premiseId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseMediaModel>> GetByPremiseIdsAsync(
            IEnumerable<Guid> premiseIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise media by premise ids.");
            var ids = premiseIds.Distinct().ToArray();
            return await _db.PremiseMedias.AsNoTracking()
                .Where(media => ids.Contains(media.PremiseMediaPremiseId))
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(
            PremiseMediaModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing premise media creation.");
            Validate(entity);

            await _db.PremiseMedias.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise media staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<PremiseMediaModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing premise media range creation.");
            var media = entities.ToList();
            foreach (var entity in media)
            {
                Validate(entity);
            }

            await _db.PremiseMedias.AddRangeAsync(media, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise media range staged for creation.");
        }

        public async Task UpdateAsync(
            PremiseMediaModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing premise media update.");
            Validate(entity);

            if (entity.PremiseMediaId <= 0)
            {
                var exception = new ArgumentException(
                    "PremiseMediaModel id must be greater than zero.", nameof(entity.PremiseMediaId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise media update rejected because its id is invalid.");
                throw exception;
            }

            var exists = await _db.PremiseMedias
                .AnyAsync(media => media.PremiseMediaId == entity.PremiseMediaId, cancellationToken);
            if (!exists)
            {
                var exception = new InvalidOperationException(
                    $"PremiseMediaModel not found! {entity.PremiseMediaId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise media update rejected because the media was not found.");
                throw exception;
            }

            _db.PremiseMedias.Update(entity);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise media staged for update.");
        }

        public void UpdateRange(
            IEnumerable<PremiseMediaModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing premise media range update.");
            var media = entities.ToList();
            foreach (var entity in media)
            {
                Validate(entity);
                if (entity.PremiseMediaId <= 0)
                {
                    var exception = new ArgumentException(
                        "PremiseMediaModel id must be greater than zero.", nameof(entities));
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Premise media range update rejected because an id is invalid.");
                    throw exception;
                }
            }

            _db.PremiseMedias.UpdateRange(media);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise media range staged for update.");
        }

        #endregion

        private void Validate(PremiseMediaModel entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.PremiseMediaPremiseId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "PremiseMediaModel premise id is required.", nameof(entity.PremiseMediaPremiseId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise media rejected because its premise id is missing.");
                throw exception;
            }

            if (string.IsNullOrWhiteSpace(entity.PremiseMediaImageUrl))
            {
                var exception = new ArgumentException(
                    "PremiseMediaModel image URL is required.", nameof(entity.PremiseMediaImageUrl));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise media rejected because its image URL is missing.");
                throw exception;
            }
        }
    }
}
