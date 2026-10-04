using Contract.Infrastructure.Persistence.DbContext;
using Contract.Interfaces.IRepository;
using Contract.Models.Invoice;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Contract.Infrastructure.Repository.ReceiptRepository
{
    public class ReceiptRepository(
        ContractDbContext context,
        ILogger<ReceiptRepository> logger,
        ILogPool logPool) : IReceiptRepository
    {
        private const string Module = "Contract";

        private const string Layer = "Infrastructure/Repository/ReceiptRepository";

        private readonly ContractDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ReceiptRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<ReceiptModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all receipts.");
            return await _db.Receipts.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<ReceiptModel?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading receipt by id.");
            return await _db.Receipts.AsNoTracking()
                .FirstOrDefaultAsync(receipt => receipt.ReceiptId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ReceiptModel>> GetByIdsAsync(IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading receipts by ids.");
            return await _db.Receipts.AsNoTracking()
                .Where(receipt => ids.Contains(receipt.ReceiptId))
                .ToListAsync(cancellationToken);
        }

        public async Task<ReceiptModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked receipt by id.");
            return await _db.Receipts.FirstOrDefaultAsync(
                receipt => receipt.ReceiptId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking receipt existence.");
            return await _db.Receipts.AnyAsync(
                receipt => receipt.ReceiptId == id, cancellationToken);
        }

        public async Task<ReceiptModel?> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading receipt by invoice id.");
            return await _db.Receipts.AsNoTracking()
                .FirstOrDefaultAsync(receipt => receipt.ReceiptInvoiceId == invoiceId,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<ReceiptModel>> GetByAccountIdAsync(Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading receipts by created by account id.");
            return await _db.Receipts.AsNoTracking()
                .Where(receipt => receipt.ReceiptCreatedByAccountId == accountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ReceiptModel>> GetByPaymentMethodAsync(
            EReceiptPaymentMethod paymentMethod,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading receipts by payment method.");
            return await _db.Receipts.AsNoTracking()
                .Where(receipt => receipt.ReceiptPaymentMethod == paymentMethod)
                .ToListAsync(cancellationToken);
        }

        public async Task<RecordBaseCursorPage<ReceiptModel>> GetApplyPagingByPaymentDateAsync(
            Guid? cursor, int pageSize, DateTime paymentDate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading receipt page by payment date.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Receipt paging rejected because the page size is invalid."));

            var startDate = paymentDate.Date;
            var endDate = startDate.AddDays(1);
            var query = _db.Receipts.AsNoTracking()
                .Where(receipt => receipt.ReceiptPaymentDate >= startDate
                                  && receipt.ReceiptPaymentDate < endDate);

            if (cursor.HasValue)
            {
                query = query.Where(receipt => receipt.ReceiptId < cursor.Value);
            }

            query = query
                .OrderByDescending(receipt => receipt.ReceiptPaymentDate)
                .ThenByDescending(receipt => receipt.ReceiptId)
                .Take(pageSize + 1);
            var receipts = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                receipts, pageSize, receipt => receipt.ReceiptId, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(ReceiptModel receiptModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing receipt creation.");
            if (receiptModel.ReceiptInvoiceId == Guid.Empty
                || receiptModel.ReceiptCreatedByAccountId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "MonthlyInvoiceModel and AccountModel id are required.", nameof(receiptModel));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Receipt creation rejected because an id is missing.");
                throw exception;
            }

            var isExisted = await _db.Receipts.AnyAsync(
                receipt => receipt.ReceiptInvoiceId == receiptModel.ReceiptInvoiceId,
                cancellationToken);
            if (isExisted)
            {
                var exception = new InvalidOperationException(
                    $"Receipt already exists for invoice {receiptModel.ReceiptInvoiceId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Receipt creation rejected because the invoice already has a receipt.");
                throw exception;
            }

            await _db.Receipts.AddAsync(receiptModel, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Receipt staged for creation.");
        }

        public async Task AddRangeAsync(IEnumerable<ReceiptModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing receipt range creation.");
            var receiptModels = entities.ToList();
            await _db.Receipts.AddRangeAsync(receiptModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Receipt range staged for creation.");
        }

        public async Task UpdateAsync(ReceiptModel receiptModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing receipt update.");
            if (receiptModel.ReceiptId == Guid.Empty)
            {
                var exception = new ArgumentException("ReceiptModel id is required.",
                    nameof(receiptModel.ReceiptId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Receipt update rejected because the receipt id is missing.");
                throw exception;
            }

            var existedReceipt = await _db.Receipts.AsNoTracking()
                .FirstOrDefaultAsync(receipt => receipt.ReceiptId == receiptModel.ReceiptId,
                    cancellationToken);
            if (existedReceipt is null)
            {
                var exception = new InvalidOperationException(
                    $"ReceiptModel not found! \n {receiptModel.ReceiptId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Receipt update rejected because the receipt was not found.");
                throw exception;
            }

            if (existedReceipt.ReceiptInvoiceId != receiptModel.ReceiptInvoiceId)
            {
                var isInvoiceReceiptExisted = await _db.Receipts.AnyAsync(
                    receipt => receipt.ReceiptInvoiceId == receiptModel.ReceiptInvoiceId
                              && receipt.ReceiptId != receiptModel.ReceiptId,
                    cancellationToken);
                if (isInvoiceReceiptExisted)
                {
                    var exception = new InvalidOperationException(
                        $"Receipt already exists for invoice {receiptModel.ReceiptInvoiceId}.");
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Receipt update rejected because the invoice already has a receipt.");
                    throw exception;
                }
            }

            _db.Receipts.Update(receiptModel);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Receipt staged for update.");
        }

        public void UpdateRange(IEnumerable<ReceiptModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing receipt range update.");
            var receiptModels = entities.ToList();
            _db.Receipts.UpdateRange(receiptModels);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Receipt range staged for update.");
        }

        #endregion
    }
}
