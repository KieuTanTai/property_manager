using Contract.Interfaces.IApplication;
using Contract.Interfaces.IRepository;
using Contract.Models.Invoice;
using Contract.Utils.Enum;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;

namespace Contract.Application
{
    public class InvoiceApplication(
        IUnitOfWork unitOfWork,
        IMonthlyInvoiceRepository invoiceRepository,
        IInvoiceDetailRepository invoiceDetailRepository,
        ILogger<InvoiceApplication> logger,
        ILogPool logPool)
        : IInvoiceApplication
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMonthlyInvoiceRepository _invoiceRepository = invoiceRepository;
        private readonly IInvoiceDetailRepository _invoiceDetailRepository = invoiceDetailRepository;
        private readonly ILogger<InvoiceApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        
        private const string Module = "Contract";
        private const string Layer = "Application";

        #region GET

        public async Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingAsync(Guid? cursor,
            int pageSize, bool isGetDetails = true, bool isGetReceipt = true,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading invoice page.");
            var result = await _invoiceRepository.GetApplyPagingAsync(
                cursor, pageSize, isGetDetails, isGetReceipt, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Invoice page loaded: {result.Items.Count} records.");
            return result;
        }

        public async Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingByInvoiceStatusAsync(Guid? cursor,
            int pageSize, EInvoiceStatus status, bool isGetDetails = true, bool isGetReceipt = true,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(status))
                throw new ArgumentException("Invoice status is invalid.", nameof(status));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading invoice page by status {status}.");
            var result = await _invoiceRepository.GetApplyPagingByStatusAsync(
                cursor, pageSize, status, isGetDetails, isGetReceipt, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Invoice page by status {status} loaded: {result.Items.Count} records.");
            return result;
        }
        
        public async Task<MonthlyInvoiceModel?> GetByInvoiceIdAsync(Guid invoiceId, bool isGetDetails = true,
            bool isGetReceipt = true, CancellationToken cancellationToken = default)
        {
            if (invoiceId == Guid.Empty)
                throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading invoice by id {invoiceId}.");
            var invoice = await _invoiceRepository.GetByIdWithNavigationAsync(invoiceId, isGetDetails, isGetReceipt, cancellationToken);
            if (invoice is null)
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"Invoice not found: {invoiceId}.");
            else
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Invoice loaded: {invoice.InvoiceId}.");
            return invoice;
        }
        
        #endregion

        #region ADD

        public async Task<MonthlyInvoiceModel> AddInvoiceAsync(MonthlyInvoiceModel invoice,
            IReadOnlyCollection<InvoiceDetailModel> details,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Creating invoice.");
            ThrowIfInvalidInvoiceModel(invoice);
            switch (details.Count)
            {
                case 0:
                    throw new ArgumentException("Invoice details are required.", nameof(details));
                case > 0 when invoice.InvoiceId == Guid.Empty:
                    throw new ArgumentException("Invoice id is required when adding invoice details.", nameof(invoice));
            }
            foreach (var detail in details)
                ThrowIfInvalidInvoiceDetailModel(detail, invoice.InvoiceId);
            
            var existingDetailsInvoice = await _invoiceDetailRepository.GetByInvoiceIdAsync(invoice.InvoiceId, cancellationToken);
            if (existingDetailsInvoice.Any())
                throw new ArgumentException("Invoice details already exist for this invoice.", nameof(details));

            await _invoiceRepository.AddAsync(invoice, cancellationToken);
            await _invoiceDetailRepository.AddRangeAsync(details, cancellationToken);
            await SaveChangesOrThrowAsync("create invoice", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Invoice created: {invoice.InvoiceId} with {details.Count} details.");
            return invoice;
        }
        
        #endregion

        #region UPDATE

        public async Task<MonthlyInvoiceModel> UpdateInvoiceAsync(MonthlyInvoiceModel invoice,
            IReadOnlyCollection<InvoiceDetailModel>? details = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating invoice.");
            ThrowIfInvalidInvoiceModel(invoice, requireId: true);
            var invoiceDetails = details?.ToList() ?? [];
            foreach (var detail in invoiceDetails)
                ThrowIfInvalidInvoiceDetailModel(detail, invoice.InvoiceId);
            foreach (var detail in invoiceDetails)
            {
                if (detail.InvoiceDetailId > 0)
                    await _invoiceDetailRepository.UpdateAsync(detail, cancellationToken);
                else
                    await _invoiceDetailRepository.AddAsync(detail, cancellationToken);
            }
            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await SaveChangesOrThrowAsync("update invoice", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Invoice updated: {invoice.InvoiceId} with {invoiceDetails.Count} details.");
            return invoice;
        }

        #endregion

        #region PRIVATE

        private async Task SaveChangesOrThrowAsync(string operation,
            CancellationToken cancellationToken)
        {
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows > 0)
            {
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Invoice operation completed: {operation}. Affected rows: {affectedRows}.");
                return;
            }

            var exception = new InvalidOperationException($"Failed to {operation}.");
            _logger.LogLayerError(_logPool, Module, Layer, exception,
                $"Invoice application failed to {operation}.");
            throw exception;
        }

        private static void ThrowIfInvalidInvoiceModel(MonthlyInvoiceModel invoice,
            bool requireId = false)
        {
            ArgumentNullException.ThrowIfNull(invoice);
            if (requireId && invoice.InvoiceId == Guid.Empty)
                throw new ArgumentException("Invoice id is required.", nameof(invoice));
            if (invoice.InvoiceContractId == Guid.Empty)
                throw new ArgumentException("Contract id is required.", nameof(invoice));
            if (invoice.InvoiceTotalAmount < 0)
                throw new ArgumentException("Invoice total amount cannot be negative.", nameof(invoice));
            if (!Enum.IsDefined(invoice.InvoiceStatus))
                throw new ArgumentException("Invoice status is invalid.", nameof(invoice));
            if (invoice.InvoiceDueDate < invoice.InvoicePaymentDate)
                throw new ArgumentException("Invoice due date must not precede its payment date.", nameof(invoice));
        }

        private static void ThrowIfInvalidInvoiceDetailModel(InvoiceDetailModel detail,
            Guid? expectedInvoiceId = null, bool requireId = false)
        {
            ArgumentNullException.ThrowIfNull(detail);
            if (requireId && detail.InvoiceDetailId <= 0)
                throw new ArgumentException("Invoice detail id is required.", nameof(detail));
            if (detail.InvoiceDetailInvoiceId == Guid.Empty || detail.InvoiceDetailPremiseId == Guid.Empty)
                throw new ArgumentException("Invoice and premise ids are required.", nameof(detail));
            if (expectedInvoiceId.HasValue && detail.InvoiceDetailInvoiceId != expectedInvoiceId.Value)
                throw new ArgumentException("Invoice detail belongs to a different invoice.", nameof(detail));
            if (detail.InvoiceDetailRentalPrice < 0 || detail.InvoiceDetailElectricityFee < 0 ||
                detail.InvoiceDetailWaterFee < 0 || detail.InvoiceDetailGarbageFee < 0)
                throw new ArgumentException("Invoice detail amounts cannot be negative.", nameof(detail));
        }

        #endregion
    }
}
