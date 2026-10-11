using Contract.Interfaces.IApplication;
using Contract.Interfaces.IRepository;
using Contract.Models.Regulation;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;

namespace Contract.Application
{
    public class RegulationApplication(
        IUnitOfWork unitOfWork,
        IRegulationRepository regulationRepository,
        ILogger<RegulationApplication> logger,
        ILogPool logPool) : IRegulationApplication
    {
        private const string Module = "Contract";
        private const string Layer = "Application";

        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IRegulationRepository _regulationRepository = regulationRepository;
        private readonly ILogger<RegulationApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        public async Task<IReadOnlyList<RegulationModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all regulations.");
            var result = await _regulationRepository.GetAllAsync(cancellationToken);
            if (result.Count != 0)
            {
                return result;
            }
            _logger.LogLayerWarning(_logPool, Module, Layer, "No regulations found.");
            throw new InvalidOperationException("No regulations found.");
        }

        public async Task<RegulationModel?> GetByIdAsync(Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation by id.");
            var result = await _regulationRepository.GetByIdAsync(regulationId, cancellationToken);
            if (result != null)
            {
                return result;
            }
            _logger.LogLayerWarning(_logPool, Module, Layer, "No regulation found by id.");
            return null;
        }

        public async Task<RegulationModel> GetByNameAsync(string regulationName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(regulationName))
            {
                throw new ArgumentException("Regulation name is required.", nameof(regulationName));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation by name.");
            var result = await _regulationRepository.GetRegulationByNameAsync(regulationName, cancellationToken);
            return result;
        }

        public async Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation page.");
            return await GetApplyingAsync( _regulationRepository.GetApplyPagingAsync, cursor, pageSize, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByNameAsync(Guid? cursor,
            int pageSize, string regulationName, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation page by name.");
            return await GetApplyingAsync((c, p, ct) => _regulationRepository.GetApplyPagingByNameAsync(c, p, regulationName, ct), cursor, pageSize, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize, bool isActive, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation page by status.");
            return await GetApplyingAsync((c, p, ct) => _regulationRepository.GetApplyPagingByStatusAsync(c, p, isActive, ct), cursor, pageSize, cancellationToken);
        }

        #endregion

        #region UPSERT

        public async Task<RegulationModel> AddAsync(RegulationModel regulation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(regulation);
            _logger.LogLayerDebug(_logPool, Module, Layer, "Adding regulation.");
            await _regulationRepository.AddAsync(regulation, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to add regulation.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Regulation creation failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Regulation created.");
            return regulation;
        }

        public async Task<int> UpdateAsync(RegulationModel regulation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(regulation);
            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating regulation.");
            await _regulationRepository.UpdateAsync(regulation, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to update regulation.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Regulation update failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Regulation updated.");
            return affectedRows;
        }

        public async Task<int> SoftDeleteAsync(Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Soft deleting regulation.");
            var regulation = await _regulationRepository.GetTrackedByIdAsync(regulationId, cancellationToken);
            if (regulation is null)
            {
                throw new ArgumentException("Regulation not found.", nameof(regulationId));
            }

            if (!regulation.RegulationIsActive)
            {
                throw new InvalidOperationException("Regulation is already deleted.");
            }

            regulation.SetRegulationIsActive(false);
            await _regulationRepository.UpdateAsync(regulation, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to delete regulation.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Regulation soft delete failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Regulation soft deleted.");
            return affectedRows;
        }

        #endregion

        #region PRIVATE

        private async Task<RecordBaseCursorPage<RegulationModel>> GetApplyingAsync(Func<Guid?, int, CancellationToken, Task<RecordBaseCursorPage<RegulationModel>>> getPageFunc, Guid? cursor, int pageSize, CancellationToken cancellationToken)
        {
            var result = await getPageFunc(cursor, pageSize, cancellationToken);
            if (result.Items.Count != 0)
            {
                return result;
            }
            _logger.LogLayerWarning(_logPool, Module, Layer, "No regulations found for the requested page.");
            throw new InvalidOperationException("No regulations found for the requested page.");
        }

        #endregion
    }
}
