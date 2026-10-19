using Premise.Interfaces.IRepository;
using Premise.Interfaces.IRepository.BusinessType;
using Premise.Interfaces.IRepository.Location;
using Premise.Interfaces.IRepository.Premise;
using Premise.Interfaces.IApplication;
using Premise.Models.Business;
using Premise.Models.Premise;
using Shared.Enum;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;

namespace Premise.Application
{
    public class PremiseApplication(
        ILogger<PremiseApplication> logger,
        ILogPool logPool,
        IPremiseRepository premiseRepository,
        IPremiseMediaRepository premiseMediaRepository,
        ILocationRepository locationRepository,
        IBusinessTypeRepository businessTypeRepository,
        IPremiseBusinessTypeRepository premiseBusinessTypeRepository,
        IUnitOfWork unitOfWork) : IPremiseApplication
    {
        private const string Module = "Premise";
        private const string Layer = "Application";

        private readonly ILogger<PremiseApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        private readonly IPremiseRepository _premiseRepository = premiseRepository;
        private readonly IPremiseMediaRepository _premiseMediaRepository = premiseMediaRepository;
        private readonly ILocationRepository _locationRepository = locationRepository;
        private readonly IBusinessTypeRepository _businessTypeRepository = businessTypeRepository;
        private readonly IPremiseBusinessTypeRepository _premiseBusinessTypeRepository = premiseBusinessTypeRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IReadOnlyList<PremiseModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all premises.");
            var result = await _premiseRepository.GetAllAsync(cancellationToken);
            if (result.Count != 0)
            {
                return result;
            }

            var exception = new InvalidOperationException("No premises found.");
            _logger.LogLayerError(_logPool, Module, Layer, exception,"Premise list request failed because no premises were found.");
            throw exception;
        }

        public async Task<PremiseModel> GetByIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            if (premiseId == Guid.Empty)
            {
                var exception = new ArgumentException("Premise id is required.", nameof(premiseId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,"Premise lookup rejected because the id is missing.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise by id.");
            var result = await _premiseRepository.GetByIdAsync(premiseId, cancellationToken);
            if (result is not null)
            {
                return result;
            }

            var notFoundException = new KeyNotFoundException($"Premise not found: {premiseId}.");
            _logger.LogLayerError(
                _logPool, Module, Layer, notFoundException,
                "Premise lookup failed because the premise was not found.");
            throw notFoundException;
        }

        public async Task<PremiseModel> GetByNameAsync(
            string premiseName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(premiseName))
            {
                var exception = new ArgumentException("Premise name is required.", nameof(premiseName));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise lookup rejected because the name is missing.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise by name.");
            return await _premiseRepository.GetPremiseByNameAsync(premiseName, cancellationToken);
        }

        public async Task<PremiseModel> GetDetailsByIdAsync(
            Guid premiseId,
            bool includeMedia = true,
            bool includeBusinessTypes = true,
            CancellationToken cancellationToken = default)
        {
            if (premiseId == Guid.Empty)
            {
                var exception = new ArgumentException("Premise id is required.", nameof(premiseId));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise details lookup rejected because the id is missing.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise details.");
            var premise = await _premiseRepository.GetPremiseAndNavigationByIdAsync(
                premiseId,
                isGetLocation: true,
                isGetMedia: includeMedia,
                isGetBusinessTypes: includeBusinessTypes,
                cancellationToken: cancellationToken);
            if (premise is not null)
            {
                return premise;
            }

            var notFoundException = new KeyNotFoundException($"Premise not found: {premiseId}.");
            _logger.LogLayerError(
                _logPool, Module, Layer, notFoundException,
                "Premise details lookup failed because the premise was not found.");
            throw notFoundException;
        }

        public async Task<IReadOnlyList<PremiseModel>> GetByFloorAsync(
            int floor,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premises by floor.");
            return await _premiseRepository.GetPremisesByFloorAsync(floor, cancellationToken);
        }

        public async Task<IReadOnlyList<PremiseModel>> GetByLocationIdAsync(
            Guid locationId,
            CancellationToken cancellationToken = default)
        {
            if (locationId == Guid.Empty)
            {
                var exception = new ArgumentException("Location id is required.", nameof(locationId));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise lookup rejected because the location id is missing.");
                throw exception;
            }

            if (!await _locationRepository.ExistsAsync(locationId, cancellationToken))
            {
                var exception = new KeyNotFoundException($"Location not found: {locationId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise lookup failed because the location was not found.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premises by location.");
            return await _premiseRepository.GetPremisesByLocationIdAsync(locationId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingAsync(
            Guid? cursor,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                var exception = new ArgumentException("Page size must be greater than 0.", nameof(pageSize));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise paging rejected because the page size is invalid.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise page.");
            var result = await _premiseRepository.GetApplyPagingAsync(cursor, pageSize, cancellationToken);
            if (result.Items.Count != 0)
            {
                return result;
            }

            var notFoundException = new InvalidOperationException("No premises found for the requested page.");
            _logger.LogLayerError(
                _logPool, Module, Layer, notFoundException,
                "Premise paging failed because no premises were found.");
            throw notFoundException;
        }

        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByStatusAsync(
            Guid? cursor,
            int pageSize,
            EPremiseStatus status,
            CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                var exception = new ArgumentException("Page size must be greater than 0.", nameof(pageSize));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise status paging rejected because the page size is invalid.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise page by status.");
            var result = await _premiseRepository.GetApplyPagingByStatusAsync(cursor, pageSize, status, cancellationToken);
            if (result.Items.Count != 0)
            {
                return result;
            }

            var notFoundException = new InvalidOperationException("No premises found for the requested status.");
            _logger.LogLayerError(
                _logPool, Module, Layer, notFoundException,
                "Premise status paging failed because no premises were found.");
            throw notFoundException;
        }

        public async Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByNameAsync(
            Guid? cursor,
            int pageSize,
            string premiseName,
            CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                var exception = new ArgumentException("Page size must be greater than 0.", nameof(pageSize));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise name paging rejected because the page size is invalid.");
                throw exception;
            }

            if (string.IsNullOrWhiteSpace(premiseName))
            {
                var exception = new ArgumentException("Premise name is required.", nameof(premiseName));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise name paging rejected because the name is missing.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise page by name.");
            return await _premiseRepository.GetApplyPagingByNameAsync(
                cursor, pageSize, premiseName, cancellationToken);
        }

        public async Task<PremiseModel> AddAsync(
            PremiseModel premise,
            CancellationToken cancellationToken = default)
        {
            if (premise is null)
            {
                var exception = new ArgumentNullException(nameof(premise));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise creation rejected because the premise is null.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Adding premise.");

            if (premise.PremiseLocationId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Location id is required.", nameof(premise.PremiseLocationId));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise creation rejected because the location id is missing.");
                throw exception;
            }

            if (!await _locationRepository.ExistsAsync(premise.PremiseLocationId, cancellationToken))
            {
                var exception = new KeyNotFoundException(
                    $"Location not found: {premise.PremiseLocationId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise creation rejected because its location was not found.");
                throw exception;
            }

            await _premiseRepository.AddAsync(premise, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to add premise.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Premise creation failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise created.");
            return premise;
        }

        public async Task<PremiseModel> UpdateAsync(
            PremiseModel premise,
            CancellationToken cancellationToken = default)
        {
            if (premise is null)
            {
                var exception = new ArgumentNullException(nameof(premise));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise update rejected because the premise is null.");
                throw exception;
            }

            if (premise.PremiseId == Guid.Empty)
            {
                var exception = new ArgumentException("Premise id is required.", nameof(premise));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise update rejected because the id is missing.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating premise.");
            var existingPremise = await _premiseRepository.GetTrackedByIdAsync(
                premise.PremiseId, cancellationToken);
            if (existingPremise is null)
            {
                var exception = new KeyNotFoundException($"Premise not found: {premise.PremiseId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise update failed because the premise was not found.");
                throw exception;
            }

            if (premise.PremiseLocationId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Location id is required.", nameof(premise.PremiseLocationId));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise update rejected because the location id is missing.");
                throw exception;
            }

            if (!await _locationRepository.ExistsAsync(premise.PremiseLocationId, cancellationToken))
            {
                var exception = new KeyNotFoundException(
                    $"Location not found: {premise.PremiseLocationId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise update rejected because its location was not found.");
                throw exception;
            }

            try
            {
                existingPremise.SetPremiseName(premise.PremiseName);
                existingPremise.SetPremiseLocationId(premise.PremiseLocationId);
                existingPremise.SetPremiseStatus(premise.PremiseStatus);
                existingPremise.SetPremisePosition(premise.PremisePosition);
                existingPremise.SetPremiseFloor(premise.PremiseFloor);
                existingPremise.SetPremiseArea(premise.PremiseArea);
                if (premise.PremiseDescription is null)
                {
                    existingPremise.ClearPremiseDescription();
                }
                else
                {
                    existingPremise.SetPremiseDescription(premise.PremiseDescription);
                }
            }
            catch (ArgumentException exception)
            {
                _logger.LogLayerError(_logPool,Module,Layer,exception,"Premise update rejected because one or more fields are invalid.");
                throw;
            }

            await _premiseRepository.UpdateAsync(existingPremise, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to update premise.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Premise update failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise updated.");
            return existingPremise;
        }

        public async Task DeleteAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            if (premiseId == Guid.Empty)
            {
                var exception = new ArgumentException("Premise id is required.", nameof(premiseId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise deletion rejected because the id is missing.");
                throw exception;
            }

            var premise = await _premiseRepository.GetTrackedByIdAsync(premiseId, cancellationToken);
            if (premise is null)
            {
                var exception = new KeyNotFoundException($"Premise not found: {premiseId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Premise deletion failed because the premise was not found.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Deleting premise.");
            await _premiseRepository.DeleteAsync(premise, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to delete premise.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Premise deletion failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise deleted.");
        }

        public async Task<PremiseMediaModel> AddImageAsync(
            Guid premiseId,
            string imageUrl,
            CancellationToken cancellationToken = default)
        {
            if (premiseId == Guid.Empty)
            {
                var exception = new ArgumentException("Premise id is required.", nameof(premiseId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,"Premise image creation rejected because the premise id is missing.");
                throw exception;
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                var exception = new ArgumentException("Image URL is required.", nameof(imageUrl));
                _logger.LogLayerError(_logPool, Module, Layer, exception,"Premise image creation rejected because the image URL is missing.");
                throw exception;
            }

            if (imageUrl.Length > 255)
            {
                var exception = new ArgumentException(
                    "Image URL cannot exceed 255 characters.", nameof(imageUrl));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise image creation rejected because the image URL is too long.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Adding image to premise.");
            if (!await _premiseRepository.ExistsAsync(premiseId, cancellationToken))
            {
                var exception = new KeyNotFoundException($"Premise not found: {premiseId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise image creation failed because the premise was not found.");
                throw exception;
            }

            var image = new PremiseMediaModel(premiseId, imageUrl);
            await _premiseMediaRepository.AddAsync(image, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to add image to premise.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Premise image creation failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Image added to premise.");
            return image;
        }

        public async Task<PremiseMediaModel> UpdateImageAsync(
            Guid premiseId,
            int imageId,
            string imageUrl,
            CancellationToken cancellationToken = default)
        {
            if (premiseId == Guid.Empty)
            {
                var exception = new ArgumentException("Premise id is required.", nameof(premiseId));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise image update rejected because the premise id is missing.");
                throw exception;
            }

            if (imageId <= 0)
            {
                var exception = new ArgumentException("Image id must be greater than zero.", nameof(imageId));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise image update rejected because the image id is invalid.");
                throw exception;
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                var exception = new ArgumentException("Image URL is required.", nameof(imageUrl));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise image update rejected because the image URL is missing.");
                throw exception;
            }

            if (imageUrl.Length > 255)
            {
                var exception = new ArgumentException(
                    "Image URL cannot exceed 255 characters.", nameof(imageUrl));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise image update rejected because the image URL is too long.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating premise image.");
            var image = await _premiseMediaRepository.GetTrackedByIdAsync(imageId, cancellationToken);
            if (image is null)
            {
                var exception = new KeyNotFoundException($"Premise image not found: {imageId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise image update failed because the image was not found.");
                throw exception;
            }

            if (image.PremiseMediaPremiseId != premiseId)
            {
                var exception = new KeyNotFoundException(
                    $"Premise image {imageId} does not belong to premise {premiseId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise image update rejected because the image belongs to another premise.");
                throw exception;
            }

            image.SetPremiseMediaImageUrl(imageUrl);
            await _premiseMediaRepository.UpdateAsync(image, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to update premise image.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Premise image update failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Premise image updated.");
            return image;
        }

        public async Task<IReadOnlyList<PremiseMediaModel>> GetImagesAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            await EnsurePremiseExistsAsync(premiseId, cancellationToken);
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise images.");
            return await _premiseMediaRepository.GetByPremiseIdAsync(premiseId, cancellationToken);
        }

        public async Task<IReadOnlyList<BusinessTypeModel>> GetBusinessTypesAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            await EnsurePremiseExistsAsync(premiseId, cancellationToken);
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading premise business types.");
            var associations = await _premiseBusinessTypeRepository.GetByPremiseIdAsync(
                premiseId, cancellationToken);
            if (associations.Count == 0)
            {
                return [];
            }

            return await _businessTypeRepository.GetByIdsAsync(
                associations.Select(association => association.BusinessTypeId),
                cancellationToken);
        }

        public async Task<PremiseBusinessTypeModel> AddBusinessTypeAsync(
            Guid premiseId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            if (businessTypeId == Guid.Empty)
            {
                var exception = new ArgumentException("Business type id is required.", nameof(businessTypeId));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise business type assignment rejected because the business type id is missing.");
                throw exception;
            }

            await EnsurePremiseExistsAsync(premiseId, cancellationToken);
            if (!await _businessTypeRepository.ExistsAsync(businessTypeId, cancellationToken))
            {
                var exception = new KeyNotFoundException($"Business type not found: {businessTypeId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise business type assignment failed because the business type was not found.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Assigning business type to premise.");
            var association = new PremiseBusinessTypeModel(premiseId, businessTypeId);
            await _premiseBusinessTypeRepository.AddAsync(association, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to assign business type to premise.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise business type assignment failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Business type assigned to premise.");
            return association;
        }

        public async Task RemoveBusinessTypeAsync(
            Guid premiseId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            if (businessTypeId == Guid.Empty)
            {
                var exception = new ArgumentException("Business type id is required.", nameof(businessTypeId));
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise business type removal rejected because the business type id is missing.");
                throw exception;
            }

            await EnsurePremiseExistsAsync(premiseId, cancellationToken);
            if (!await _premiseBusinessTypeRepository.ExistsAsync(
                    premiseId, businessTypeId, cancellationToken))
            {
                var exception = new KeyNotFoundException(
                    $"Business type {businessTypeId} is not assigned to premise {premiseId}.");
                _logger.LogLayerError(
                    _logPool, Module, Layer, exception,
                    "Premise business type removal failed because the association was not found.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Removing business type from premise.");
            await _premiseBusinessTypeRepository.DeleteAsync(
                premiseId, businessTypeId, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to remove business type from premise.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,"Premise business type removal failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Business type removed from premise.");
        }

        private async Task EnsurePremiseExistsAsync(
            Guid premiseId,
            CancellationToken cancellationToken)
        {
            if (premiseId == Guid.Empty)
            {
                var exception = new ArgumentException("Premise id is required.", nameof(premiseId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,"Premise lookup rejected because the id is missing.");
                throw exception;
            }

            if (!await _premiseRepository.ExistsAsync(premiseId, cancellationToken))
            {
                var exception = new KeyNotFoundException($"Premise not found: {premiseId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,"Premise lookup failed because the premise was not found.");
                throw exception;
            }
        }
    }
}