using Microsoft.EntityFrameworkCore;
using Premise.Infrastructures.Persistence.DbContext;
using Premise.Interfaces.Location;
using Premise.Models.Premise;
using Shared.Logging;

namespace Premise.Infrastructures.Repository.PremiseRepository
{
    public class LocationRepository(
        PremiseDbContext context,
        ILogger<LocationRepository> logger,
        ILogPool logPool) : ILocationRepository
    {
        private const string Module = "Premise";

        private const string Layer = "Infrastructures/Repository/LocationRepository";

        private readonly PremiseDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<LocationRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<LocationModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all locations.");
            return await _db.Locations.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<LocationModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading location by id.");
            return await _db.Locations.AsNoTracking()
                .FirstOrDefaultAsync(location => location.LocationId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<LocationModel>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading locations by ids.");
            return await _db.Locations.AsNoTracking()
                .Where(location => ids.Contains(location.LocationId))
                .ToListAsync(cancellationToken);
        }

        public async Task<LocationModel?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked location by id.");
            return await _db.Locations
                .FirstOrDefaultAsync(location => location.LocationId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking location existence.");
            return await _db.Locations
                .AnyAsync(location => location.LocationId == id, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(LocationModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing location creation.");
            if (string.IsNullOrWhiteSpace(entity.LocationAddress))
            {
                var exception = new ArgumentException("LocationModel address is required.", nameof(entity.LocationAddress));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Location creation rejected because the address is missing.");
                throw exception;
            }

            await _db.Locations.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Location staged for creation.");
        }

        public async Task UpdateAsync(LocationModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing location update.");
            if (entity.LocationId == Guid.Empty)
            {
                var exception = new ArgumentException("LocationModel id is required.", nameof(entity.LocationId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Location update rejected because the id is missing.");
                throw exception;
            }

            var existedLocation = await _db.Locations.AsNoTracking()
                .FirstOrDefaultAsync(location => location.LocationId == entity.LocationId, cancellationToken);

            if (existedLocation is null)
            {
                var exception = new InvalidOperationException($"LocationModel not found! \n {entity.LocationId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Location update rejected because the location was not found.");
                throw exception;
            }

            _db.Locations.Update(entity);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Location staged for update.");
        }

        #endregion
    }
}
