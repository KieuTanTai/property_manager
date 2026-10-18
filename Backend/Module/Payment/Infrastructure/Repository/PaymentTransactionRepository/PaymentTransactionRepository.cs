using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence.DbContext;
using Payment.Interfaces.IRepository;
using Payment.Models.Payment;
using Payment.Utils.Enum;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Payment.Infrastructure.Repository.PaymentTransactionRepository
{
    public class PaymentTransactionRepository(
        PaymentDbContext context,
        ILogger<PaymentTransactionRepository> logger,
        ILogPool logPool) : IPaymentTransactionRepository
    {
        private const string Module = "Payment";

        private const string Layer = "Infrastructure/Repository/PaymentTransactionRepository";

        private readonly PaymentDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<PaymentTransactionRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<PaymentTransactionModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all payment transactions.");
            return await _db.PaymentTransactions.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<PaymentTransactionModel?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transaction by id.");
            return await _db.PaymentTransactions.AsNoTracking()
                .FirstOrDefaultAsync(transaction => transaction.PaymentTransactionId == id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentTransactionModel>> GetByIdsAsync(IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transactions by ids.");
            return await _db.PaymentTransactions.AsNoTracking()
                .Where(transaction => ids.Contains(transaction.PaymentTransactionId))
                .ToListAsync(cancellationToken);
        }

        public async Task<PaymentTransactionModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked payment transaction by id.");
            return await _db.PaymentTransactions.FirstOrDefaultAsync(
                transaction => transaction.PaymentTransactionId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking payment transaction existence.");
            return await _db.PaymentTransactions.AnyAsync(
                transaction => transaction.PaymentTransactionId == id, cancellationToken);
        }

        public async Task<PaymentTransactionModel?> GetByTxnRefAsync(string txnRef,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transaction by txn ref.");
            return await _db.PaymentTransactions.AsNoTracking()
                .FirstOrDefaultAsync(transaction => transaction.PaymentTxnRef == txnRef,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentTransactionModel>> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transactions by invoice id.");
            return await _db.PaymentTransactions.AsNoTracking()
                .Where(transaction => transaction.PaymentInvoiceId == invoiceId)
                .OrderByDescending(transaction => transaction.PaymentCreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentTransactionModel>> GetByAccountIdAsync(Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transactions by account id.");
            return await _db.PaymentTransactions.AsNoTracking()
                .Where(transaction => transaction.PaymentAccountId == accountId)
                .OrderByDescending(transaction => transaction.PaymentCreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingAsync(Guid? cursor,
            int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transaction page.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Payment transaction paging rejected because the page size is invalid."));
            var query = _db.PaymentTransactions.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(transaction => transaction.PaymentTransactionId < cursor.Value);
            }

            query = query.OrderByDescending(transaction => transaction.PaymentTransactionId)
                .Take(pageSize + 1);
            var transactions = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                transactions, pageSize, transaction => transaction.PaymentTransactionId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingByAccountIdAsync(
            Guid? cursor, int pageSize, Guid accountId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading payment transaction page by account id.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Payment transaction paging rejected because the page size is invalid."));
            var query = _db.PaymentTransactions.AsNoTracking()
                .Where(transaction => transaction.PaymentAccountId == accountId);

            if (cursor.HasValue)
            {
                query = query.Where(transaction => transaction.PaymentTransactionId < cursor.Value);
            }

            query = query.OrderByDescending(transaction => transaction.PaymentTransactionId)
                .Take(pageSize + 1);
            var transactions = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                transactions, pageSize, transaction => transaction.PaymentTransactionId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingByStatusAsync(
            Guid? cursor, int pageSize, EPaymentStatus status,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading payment transaction page by status.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Payment transaction paging rejected because the page size is invalid."));
            var query = _db.PaymentTransactions.AsNoTracking()
                .Where(transaction => transaction.PaymentStatus == status);

            if (cursor.HasValue)
            {
                query = query.Where(transaction => transaction.PaymentTransactionId < cursor.Value);
            }

            query = query.OrderByDescending(transaction => transaction.PaymentTransactionId)
                .Take(pageSize + 1);
            var transactions = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                transactions, pageSize, transaction => transaction.PaymentTransactionId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(PaymentTransactionModel paymentTransaction,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing payment transaction creation.");
            if (paymentTransaction.PaymentInvoiceId == Guid.Empty
                || paymentTransaction.PaymentAccountId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "InvoiceModel and AccountModel id are required.", nameof(paymentTransaction));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Payment transaction creation rejected because an id is missing.");
                throw exception;
            }

            if (paymentTransaction.PaymentAmount <= 0)
            {
                var exception = new ArgumentException(
                    "Payment amount must be greater than zero.", nameof(paymentTransaction.PaymentAmount));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Payment transaction creation rejected because the amount is invalid.");
                throw exception;
            }

            await _db.PaymentTransactions.AddAsync(paymentTransaction, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Payment transaction staged for creation.");
        }

        public async Task AddRangeAsync(IEnumerable<PaymentTransactionModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing payment transaction range creation.");
            var transactions = entities.ToList();
            await _db.PaymentTransactions.AddRangeAsync(transactions, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Payment transaction range staged for creation.");
        }

        public async Task UpdateAsync(PaymentTransactionModel paymentTransaction,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing payment transaction update.");
            if (paymentTransaction.PaymentTransactionId == Guid.Empty)
            {
                var exception = new ArgumentException("PaymentTransactionModel id is required.",
                    nameof(paymentTransaction.PaymentTransactionId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Payment transaction update rejected because the transaction id is missing.");
                throw exception;
            }

            var existedTransaction = await _db.PaymentTransactions.AsNoTracking()
                .FirstOrDefaultAsync(
                    transaction => transaction.PaymentTransactionId == paymentTransaction.PaymentTransactionId,
                    cancellationToken);
            if (existedTransaction is null)
            {
                var exception = new InvalidOperationException(
                    $"PaymentTransactionModel not found! \n {paymentTransaction.PaymentTransactionId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Payment transaction update rejected because the transaction was not found.");
                throw exception;
            }

            _db.PaymentTransactions.Update(paymentTransaction);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Payment transaction staged for update.");
        }

        public void UpdateRange(IEnumerable<PaymentTransactionModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing payment transaction range update.");
            var transactions = entities.ToList();
            _db.PaymentTransactions.UpdateRange(transactions);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Payment transaction range staged for update.");
        }

        /// <inheritdoc />
        public async Task<int> SettleByTxnRefAsync(string txnRef,
            PaymentSettlementModel settlement, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Settling payment transaction by txn ref.");

            // One statement, with the settleable-state check inside it, so the settle is atomic: a
            // concurrent IPN finds nothing to update instead of overwriting this result from a
            // stale snapshot. Expired rows are only revived by a success, because that status came
            // from this module's timeout rather than from the gateway.
            var affectedRows = await _db.PaymentTransactions
                .Where(transaction => transaction.PaymentTxnRef == txnRef
                                      && (transaction.PaymentStatus == EPaymentStatus.Pending
                                          || (transaction.PaymentStatus == EPaymentStatus.Expired
                                              && settlement.Status == EPaymentStatus.Succeeded)))
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(transaction => transaction.PaymentStatus, settlement.Status)
                        .SetProperty(transaction => transaction.PaymentGatewayTransactionNumber,
                            settlement.GatewayTransactionNumber)
                        .SetProperty(transaction => transaction.PaymentBankCode, settlement.BankCode)
                        .SetProperty(transaction => transaction.PaymentBankTranNo, settlement.BankTranNo)
                        .SetProperty(transaction => transaction.PaymentResponseCode, settlement.ResponseCode)
                        .SetProperty(transaction => transaction.PaymentTransactionStatusCode,
                            settlement.TransactionStatusCode)
                        .SetProperty(transaction => transaction.PaymentCardType, settlement.CardType)
                        .SetProperty(transaction => transaction.PaymentOrderInfo, settlement.OrderInfo)
                        .SetProperty(transaction => transaction.PaymentPayDate, settlement.PayDate)
                        .SetProperty(transaction => transaction.PaymentUpdatedAt, DateTime.UtcNow),
                    cancellationToken);

            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Payment transaction {txnRef} settlement affected {affectedRows} row(s).");
            return affectedRows;
        }

        /// <inheritdoc />
        public async Task<int> ExpireStalePendingAsync(DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Expiring abandoned pending payment transactions.");

            var affectedRows = await _db.PaymentTransactions
                .Where(transaction => transaction.PaymentStatus == EPaymentStatus.Pending
                                      && transaction.PaymentExpiresAt < utcNow)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(transaction => transaction.PaymentStatus, EPaymentStatus.Expired)
                        .SetProperty(transaction => transaction.PaymentUpdatedAt, DateTime.UtcNow),
                    cancellationToken);

            if (affectedRows > 0)
            {
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Expired {affectedRows} abandoned pending payment transaction(s).");
            }

            return affectedRows;
        }

        #endregion
    }
}
