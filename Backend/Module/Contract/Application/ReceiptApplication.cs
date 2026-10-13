using System.Text.RegularExpressions;
using Contract.Interfaces.IApplication;
using Contract.Interfaces.IRepository;
using Contract.Models.Invoice;
using Contract.Utils.Enum;
using Shared.Enum;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;

namespace Contract.Application
{
    public class ReceiptApplication(
        IUnitOfWork unitOfWork,
        IReceiptRepository receiptRepository,
        IMonthlyInvoiceRepository invoiceRepository,
        ILogger<ReceiptApplication> logger,
        ILogPool logPool) : IReceiptApplication
    {
        private const string Module = "Contract";
        private const string Layer = "Application";

        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IReceiptRepository _receiptRepository = receiptRepository;
        private readonly IMonthlyInvoiceRepository _invoiceRepository = invoiceRepository;
        private readonly ILogger<ReceiptApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        public async Task<IReadOnlyList<ReceiptModel>> GetAllReceiptsAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all receipts.");
            var result = await _receiptRepository.GetAllAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"All receipts loaded: {result.Count} records.");
            return result;
        }

        public async Task<ReceiptModel?> GetByReceiptIdAsync(Guid receiptId,
            CancellationToken cancellationToken = default)
        {
            ThrowIfEmpty(receiptId, nameof(receiptId));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading receipt {receiptId}.");
            var result = await _receiptRepository.GetByIdAsync(receiptId, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipt lookup completed for {receiptId}: {(result is null ? "not found" : "found")}.");
            return result;
        }

        public async Task<IReadOnlyList<ReceiptModel>> GetReceiptsByIdsAsync(IEnumerable<Guid> receiptIds,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(receiptIds);
            var ids = receiptIds.Distinct().ToArray();
            if (ids.Length == 0 || ids.Any(id => id == Guid.Empty))
                throw new ArgumentException("At least one valid receipt id is required.", nameof(receiptIds));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading receipts by {ids.Length} ids.");
            var result = await _receiptRepository.GetByIdsAsync(ids, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipts loaded by ids: {result.Count} records.");
            return result;
        }

        public async Task<ReceiptModel?> GetReceiptByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            ThrowIfEmpty(invoiceId, nameof(invoiceId));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading receipt for invoice {invoiceId}.");
            var result = await _receiptRepository.GetByInvoiceIdAsync(invoiceId, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipt lookup for invoice {invoiceId} completed: {(result is null ? "not found" : "found")}.");
            return result;
        }

        public async Task<IReadOnlyList<ReceiptModel>> GetReceiptsByAccountIdAsync(Guid accountId,
            CancellationToken cancellationToken = default)
        {
            ThrowIfEmpty(accountId, nameof(accountId));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading receipts created by account {accountId}.");
            var result = await _receiptRepository.GetByAccountIdAsync(accountId, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipts by account {accountId} loaded: {result.Count} records.");
            return result;
        }

        public async Task<IReadOnlyList<ReceiptModel>> GetReceiptsByPaymentMethodAsync(
            EReceiptPaymentMethod paymentMethod,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(paymentMethod))
                throw new ArgumentException("Receipt payment method is invalid.", nameof(paymentMethod));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading receipts by payment method {paymentMethod}.");
            var result = await _receiptRepository.GetByPaymentMethodAsync(paymentMethod, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipts by payment method {paymentMethod} loaded: {result.Count} records.");
            return result;
        }

        public async Task<RecordBaseCursorPage<ReceiptModel>> GetApplyPagingByReceiptPaymentDateAsync(
            Guid? cursor, int pageSize, DateTime paymentDate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Loading receipt page for {paymentDate:yyyy-MM-dd}.");
            var result = await _receiptRepository.GetApplyPagingByPaymentDateAsync(
                cursor, pageSize, paymentDate, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipt page for {paymentDate:yyyy-MM-dd} loaded: {result.Items.Count} records.");
            return result;
        }

        public async Task<bool> ExistsAsync(Guid receiptId,
            CancellationToken cancellationToken = default)
        {
            ThrowIfEmpty(receiptId, nameof(receiptId));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Checking whether receipt {receiptId} exists.");
            var result = await _receiptRepository.ExistsAsync(receiptId, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipt existence check completed for {receiptId}: {result}.");
            return result;
        }

        #endregion

        #region ADD

        public async Task<ReceiptModel> AddReceiptAsync(ReceiptModel receipt,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Creating receipt.");
            ThrowIfInvalidReceiptModel(receipt);
            var invoice = await _invoiceRepository.GetTrackedByIdAsync(
                receipt.ReceiptInvoiceId, cancellationToken);
            if (invoice is null)
                throw new ArgumentException("Invoice was not found.", nameof(receipt));
            if (invoice.InvoiceStatus != EInvoiceStatus.Paid)
                throw new InvalidOperationException(
                    "Cannot create receipt for an invoice that is not marked as paid.");

            await _receiptRepository.AddAsync(receipt, cancellationToken);
            invoice.SetPaymentDate(receipt.ReceiptPaymentDate);
            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await SaveChangesOrThrowAsync("create receipt", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipt created: {receipt.ReceiptId} for invoice {receipt.ReceiptInvoiceId}.");
            return receipt;
        }

        #endregion

        #region UPDATE

        public async Task<ReceiptModel> UpdateReceiptAsync(Guid receiptId, Guid invoiceId, DateTime paymentDate, decimal amount, string? transactionInfo,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating receipt.");
            ThrowIfInvalidReceiptUpdate(receiptId, invoiceId, paymentDate, amount, transactionInfo);

            var existingReceipt = await _receiptRepository.GetByIdAsync(receiptId, cancellationToken);
            if (existingReceipt is null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"Receipt update rejected because receipt {receiptId} was not found.");
                throw new ArgumentException("Receipt was not found.", nameof(receiptId));
            }
            if (existingReceipt.ReceiptInvoiceId != invoiceId)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"Receipt update rejected because receipt {receiptId} does not belong to invoice {invoiceId}.");
                throw new ArgumentException("Receipt does not belong to the specified invoice.", nameof(invoiceId));
            }

            var invoice = await _invoiceRepository.GetTrackedByIdAsync(invoiceId, cancellationToken);
            if (invoice is null)
                throw new ArgumentException("Invoice was not found.", nameof(invoiceId));
            if (invoice.InvoiceStatus != EInvoiceStatus.Paid)
                throw new InvalidOperationException(
                    "Cannot update receipt for an invoice that is not marked as paid.");

            // Only these receipt fields are mutable through this operation.
            existingReceipt.SetPaymentDate(paymentDate);
            existingReceipt.SetAmount(amount);
            existingReceipt.SetTransactionInfo(transactionInfo);
            invoice.SetPaymentDate(paymentDate);
            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await _receiptRepository.UpdateAsync(existingReceipt, cancellationToken);
            await SaveChangesOrThrowAsync("update receipt", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Receipt updated: {existingReceipt.ReceiptId} for invoice {existingReceipt.ReceiptInvoiceId}.");
            return existingReceipt;
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
                    $"Receipt operation completed: {operation}. Affected rows: {affectedRows}.");
                return;
            }

            var exception = new InvalidOperationException($"Failed to {operation}.");
            _logger.LogLayerError(_logPool, Module, Layer, exception,
                $"Receipt application failed to {operation}.");
            throw exception;
        }

        private static void ThrowIfEmpty(Guid id, string parameterName)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", parameterName);
        }

        private static void ThrowIfInvalidReceiptModel(ReceiptModel receipt)
        {
            ArgumentNullException.ThrowIfNull(receipt);
            if (receipt.ReceiptId == Guid.Empty)
                throw new ArgumentException("Receipt id is required.", nameof(receipt));
            if (receipt.ReceiptInvoiceId == Guid.Empty || receipt.ReceiptCreatedByAccountId == Guid.Empty)
                throw new ArgumentException("Invoice and creator account ids are required.", nameof(receipt));
            if (receipt.ReceiptAmount <= 0)
                throw new ArgumentException("Receipt amount must be greater than zero.", nameof(receipt));
            if (!Enum.IsDefined(receipt.ReceiptPaymentMethod))
                throw new ArgumentException("Receipt payment method is invalid.", nameof(receipt));
            if (receipt.ReceiptTransactionReference?.Length > 100 ||
                receipt.ReceiptGatewayTransactionNumber?.Length > 100 ||
                receipt.ReceiptTransactionInfo?.Length > 250 || receipt.ReceiptBankName?.Length > 100)
                throw new ArgumentException("One or more receipt fields exceed their maximum length.", nameof(receipt));
        }

        private static void ThrowIfInvalidReceiptUpdate(Guid receiptId, Guid invoiceId,
            DateTime paymentDate, decimal amount, string? transactionInfo)
        {
            ThrowIfEmpty(receiptId, nameof(receiptId));
            ThrowIfEmpty(invoiceId, nameof(invoiceId));

            if (amount <= 0)
                throw new ArgumentException("Receipt amount must be greater than zero.", nameof(amount));
            if (paymentDate <= DateTime.MinValue || paymentDate >= DateTime.MaxValue ||
                paymentDate > DateTime.Now)
                throw new ArgumentException("Payment date is invalid.", nameof(paymentDate));
            if (transactionInfo?.Length > 250)
                throw new ArgumentException("Transaction info must not exceed 250 characters.", nameof(transactionInfo));
            if (transactionInfo is not null &&
                !Regex.IsMatch(
                    transactionInfo, @"^[\p{L}\p{N} -]*$",
                    RegexOptions.CultureInvariant))
                throw new ArgumentException(
                    "Transaction info may contain only letters, numbers, spaces, and hyphens.",
                    nameof(transactionInfo));
        }
        #endregion
    }
}
