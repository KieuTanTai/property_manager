using Contract.Interfaces.IApplication;
using Contract.Interfaces.IRepository;
using Contract.Models.Contract;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;

namespace Contract.Application
{
    public class ViolationApplication(
        IUnitOfWork unitOfWork,
        IContractViolationRepository violationRepository,
        IInvoiceApplication invoiceApplication,
        IReceiptApplication receiptApplication,
        ILogger<ViolationApplication> logger,
        ILogPool logPool) : IViolationApplication
    {
        private const string Module = "Contract";

        private const string Layer = "Application";

        private readonly IInvoiceApplication _invoiceApplication = invoiceApplication;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ViolationApplication> _logger = logger;

        private readonly IReceiptApplication _receiptApplication = receiptApplication;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private readonly IContractViolationRepository _violationRepository = violationRepository;

        #region CREATE

        public async Task<int> AddViolationAsync(ContractViolationModel violation,
            CancellationToken cancellationToken = default)
        {
            ValidateViolation(violation, false);
            if (violation.ViolationIsResolved)
            {
                throw new ArgumentException("Cannot create a resolved violation.", nameof(violation));
            }
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Creating contract violation for contract {violation.ContractId}.");

            await _violationRepository.AddAsync(violation, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync("create", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contract violation {violation.ViolationId} created for contract {violation.ContractId}.");
            return affectedRows;
        }

        #endregion

        #region UPDATE

        public async Task<int> UpdateViolationAsync(ContractViolationModel violation,
            CancellationToken cancellationToken = default)
        {
            ValidateViolation(violation, true);
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Updating contract violation {violation.ViolationId}.");

            await _violationRepository.UpdateAsync(violation, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync("update", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contract violation {violation.ViolationId} updated.");
            return affectedRows;
        }

        #endregion

        #region DELETE

        public async Task<int> DeleteViolationAsync(Guid violationId,
            CancellationToken cancellationToken = default)
        {
            if (violationId == Guid.Empty)
            {
                throw new ArgumentException("Violation id is required.", nameof(violationId));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Deleting contract violation {violationId}.");
            await _violationRepository.DeleteAsync(violationId, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync("delete", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contract violation {violationId} deleted.");
            return affectedRows;
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<ContractViolationModel>> GetViolationsByContractIdAsync(
            Guid contractId, CancellationToken cancellationToken = default)
        {
            if (contractId == Guid.Empty)
            {
                throw new ArgumentException("Contract id is required.", nameof(contractId));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading violations for contract {contractId}.");

            var violations = await _violationRepository.GetByContractIdAsync(contractId, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Loaded {violations.Count} violations for contract {contractId}.");
            return violations;
        }

        public async Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingAsync(
            Guid? cursor, int pageSize, CancellationToken cancellationToken = default)
        {
            ValidatePageSize(pageSize);
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract violation page.");

            var page = await _violationRepository.GetApplyPagingAsync(cursor, pageSize, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contract violation page loaded: {page.Items.Count} records.");
            return page;
        }

        public async Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByViolationContentAsync(
            Guid? cursor, int pageSize, string violationContent,
            CancellationToken cancellationToken = default)
        {
            ValidatePageSize(pageSize);
            if (string.IsNullOrWhiteSpace(violationContent))
            {
                throw new ArgumentException("Violation content is required.", nameof(violationContent));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract violation page by content.");
            var page = await _violationRepository.GetApplyPagingByViolationContentAsync(
                cursor, pageSize, violationContent.Trim(), cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contract violation page by content loaded: {page.Items.Count} records.");
            return page;
        }

        public async Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByViolationDateRangeAsync(
            Guid? cursor, int pageSize, DateTime minViolationDate, DateTime maxViolationDate,
            CancellationToken cancellationToken = default)
        {
            ValidatePageSize(pageSize);
            if (minViolationDate > maxViolationDate)
            {
                throw new ArgumentException("Minimum violation date must be on or before maximum violation date.",
                    nameof(minViolationDate));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading contract violation page from {minViolationDate:O} to {maxViolationDate:O}.");
            var page = await _violationRepository.GetApplyPagingByRangeViolationDateAsync(
                cursor, pageSize, minViolationDate, maxViolationDate, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contract violation page by date range loaded: {page.Items.Count} records.");
            return page;
        }

        public async Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByViolationStatusAsync(
            Guid? cursor, int pageSize, bool isResolved, CancellationToken cancellationToken = default)
        {
            ValidatePageSize(pageSize);
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading contract violation page by resolved status ({isResolved}).");
            var page = await _violationRepository.GetApplyPagingByStatusAsync(
                cursor, pageSize, isResolved, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contract violation page by status loaded: {page.Items.Count} records.");
            return page;
        }

        #endregion

        #region PRIVATE

        private async Task<int> SaveChangesOrThrowAsync(string operation,
            CancellationToken cancellationToken)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Saving changes after contract violation {operation}.");
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows > 0)
            {
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Contract violation {operation} saved. Affected rows: {affectedRows}.");
                return affectedRows;
            }

            var exception = new InvalidOperationException(
                $"Failed to {operation} contract violation; no records were changed.");
            _logger.LogLayerError(_logPool, Module, Layer, exception,
                $"Contract violation {operation} failed.");
            throw exception;
        }

        private void ValidatePageSize(int pageSize)
        {
            if (pageSize > 0)
            {
                return;
            }

            var exception = new ArgumentOutOfRangeException(nameof(pageSize),
                "Page size must be greater than zero.");
            _logger.LogLayerError(_logPool, Module, Layer, exception,
                "Contract violation paging rejected because page size is invalid.");
            throw exception;
        }

        private static void ValidateViolation(ContractViolationModel violation, bool requireId)
        {
            ArgumentNullException.ThrowIfNull(violation);
            if (violation.ContractId == Guid.Empty)
            {
                throw new ArgumentException("Contract id is required.", nameof(violation));
            }

            if (requireId)
            {
                if (violation.ViolationId == Guid.Empty)
                {
                    throw new ArgumentException("Violation id is required.", nameof(violation));
                }
            }

            if (string.IsNullOrWhiteSpace(violation.ViolationContent))
            {
                throw new ArgumentException("Violation content is required.", nameof(violation));
            }

            if (violation.ViolationPenaltyAmount < 0)
            {
                throw new ArgumentException("Violation penalty amount cannot be negative.", nameof(violation));
            }

            if (violation.ViolationDueDate < violation.ViolationDate)
            {
                throw new ArgumentException("Violation due date must be on or after violation date.",
                    nameof(violation));
            }
        }

        #endregion
    }
}