using Premise.Interfaces.IRepository.Location;
using Premise.Interfaces.IApplication;
using Premise.Models.Premise;
using Shared.Interfaces;
using Shared.Logging;

namespace Premise.Application
{
    public partial class LocationApplication(
        ILogger<LocationApplication> logger,
        ILogPool logPool,
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork
    ) : ILocationApplication
    {
        private const string Module = "Premise";
        private const string Layer = "Application";

        private readonly ILogger<LocationApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        private readonly ILocationRepository _locationRepository = locationRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IReadOnlyList<LocationModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all locations.");
            var result = await _locationRepository.GetAllAsync(cancellationToken);
            if (result.Count != 0)
            {
                return result;
            }

            _logger.LogLayerWarning(_logPool, Module, Layer, "No locations found.");
            throw new InvalidOperationException("No locations found.");
        }

        public async Task<LocationModel> GetByIdAsync(
            Guid locationId,
            CancellationToken cancellationToken = default)
        {
            if (locationId == Guid.Empty)
            {
                throw new ArgumentException("Location id is required.", nameof(locationId));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading location by id.");
            var result = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
            if (result is not null)
            {
                return result;
            }

            _logger.LogLayerWarning(_logPool, Module, Layer, "Location not found by id.");
            throw new KeyNotFoundException($"Location not found: {locationId}.");
        }

        public async Task<LocationModel> AddAsync(
            string locationAddress,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(locationAddress))
            {
                throw new ArgumentException("Location address is required.", nameof(locationAddress));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Adding location.");
            var location = new LocationModel(locationAddress);
            await _locationRepository.AddAsync(location, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to add location.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Location creation failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Location created.");
            return location;
        }

        public async Task<LocationModel> UpdateAsync(
            Guid locationId,
            string locationAddress,
            CancellationToken cancellationToken = default)
        {
            if (locationId == Guid.Empty)
            {
                throw new ArgumentException("Location id is required.", nameof(locationId));
            }

            if (string.IsNullOrWhiteSpace(locationAddress))
            {
                throw new ArgumentException("Location address is required.", nameof(locationAddress));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating location.");
            var location = await _locationRepository.GetTrackedByIdAsync(locationId, cancellationToken)
                ?? throw new KeyNotFoundException($"Location not found: {locationId}.");

            location.SetLocationAddress(locationAddress);
            await _locationRepository.UpdateAsync(location, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to update location.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Location update failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Location updated.");
            return location;
        }

        public async Task DeleteAsync(
            Guid locationId,
            CancellationToken cancellationToken = default)
        {
            if (locationId == Guid.Empty)
            {
                throw new ArgumentException("Location id is required.", nameof(locationId));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Deleting location.");
            var existingLocation = await _locationRepository.GetTrackedByIdAsync(locationId, cancellationToken)
                ?? throw new KeyNotFoundException($"Location not found: {locationId}.");

            await _locationRepository.SoftDeleteAsync(locationId, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to delete location.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Location deletion failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Location deleted.");
        }
    }
}
