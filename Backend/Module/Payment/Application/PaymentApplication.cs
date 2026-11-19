using System.Globalization;
using Microsoft.AspNetCore.Http;
using Payment.Interfaces.IApplication;
using Payment.Interfaces.IRepository;
using Payment.Models.Payment;
using Payment.Utils;
using Payment.Utils.Enum;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;

namespace Payment.Application
{
    public class PaymentApplication(
        IPaymentTransactionRepository paymentTransactionRepository,
        IUnitOfWork unitOfWork,
        IVnpay vnpay,
        VnpaySettingsModel vnpaySettings,
        ILogger<PaymentApplication> logger,
        ILogPool logPool) : IPaymentApplication
    {
        private const string Module = "Payment";

        private const string Layer = "Application";

        // VNPAY only accepts amounts between 5.000 and 1.000.000.000 VND.
        private const decimal MinAmount = 5_000m;

        private const decimal MaxAmount = 1_000_000_000m;

        private const int PaymentLifetimeMinutes = 15;

        // VNPAY codes worth naming, because these three decide whether a transaction is final.
        private const string IncompleteTransactionStatusCode = "01";

        private const string DebitedNotSettledTransactionStatusCode = "04";

        private const string ProcessingTransactionStatusCode = "05";

        private const string DebitedFraudResponseCode = "07";

        private readonly IPaymentTransactionRepository _paymentTransactionRepository =
            paymentTransactionRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private readonly IVnpay _vnpay = vnpay;

        private readonly VnpaySettingsModel _vnpaySettings = vnpaySettings;

        private readonly ILogger<PaymentApplication> _logger = logger;

        private readonly ILogPool _logPool = logPool;

        #region WRITE

        public async Task<PaymentUrlResult> CreatePaymentUrlAsync(Guid invoiceId, Guid accountId,
            decimal amount, string? description, string? bankCode, string? language, string ipAddress,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Creating VNPAY payment url.");

            if (invoiceId == Guid.Empty)
            {
                throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
            }

            if (accountId == Guid.Empty)
            {
                throw new ArgumentException("Account id is required.", nameof(accountId));
            }

            if (amount < MinAmount || amount > MaxAmount)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount,
                    $"Payment amount must be between {MinAmount} and {MaxAmount} VND.");
            }

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentException("Client ip address is required.", nameof(ipAddress));
            }

            var orderInfo = PaymentDescriptionHelper.Sanitize(description);
            if (string.IsNullOrWhiteSpace(orderInfo))
            {
                orderInfo = $"Thanh toan hoa don {invoiceId}";
            }

            // vnp_TxnRef must be unique per transaction and numeric, so the ticks value is used as
            // the transaction reference and stored; it is the key an IPN comes back with.
            var txnRef = DateTime.UtcNow.Ticks;

            var vnpayRequest = new PaymentRequest
            {
                PaymentId = txnRef,
                Description = orderInfo,
                Money = (double)amount,
                IpAddress = ipAddress,
                BankCode = ResolveBankCode(bankCode),
                // VNPAY expects vnp_CreateDate in GMT+7. It does not convert, so derive it from UTC
                // instead of DateTime.Now to stay correct whatever the host timezone is.
                CreatedDate = DateTime.UtcNow.AddHours(7),
                Currency = Currency.VND,
                Language = ResolveLanguage(language)
            };

            var paymentUrl = _vnpay.GetPaymentUrl(vnpayRequest);

            var transaction = new PaymentTransactionModel(invoiceId, accountId, amount, orderInfo,
                DateTime.UtcNow.AddMinutes(PaymentLifetimeMinutes));
            transaction.SetTxnRef(txnRef.ToString(CultureInfo.InvariantCulture));
            transaction.SetStatus(EPaymentStatus.Pending);

            await _paymentTransactionRepository.AddAsync(transaction, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to store payment transaction.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Payment transaction creation failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Payment transaction {transaction.PaymentTransactionId} created for invoice {invoiceId}.");
            return new PaymentUrlResult(transaction, paymentUrl);
        }

        public async Task<IpnResult> ProcessIpnAsync(IQueryCollection query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            _logger.LogLayerDebug(_logPool, Module, Layer, "Processing VNPAY IPN request.");

            var txnRef = QueryValue(query, "vnp_TxnRef");
            if (string.IsNullOrWhiteSpace(txnRef))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "IPN rejected because vnp_TxnRef is missing.");
                return new IpnResult(1, "Order not found");
            }

            var transaction = await _paymentTransactionRepository.GetByTxnRefAsync(txnRef, cancellationToken);
            if (transaction is null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"IPN rejected because transaction {txnRef} was not found.");
                return new IpnResult(1, "Order not found");
            }

            // Expired is this module's own timeout rather than a gateway verdict, so a signed
            // success arriving afterwards still has to be able to settle the row; everything else
            // final is left alone (see SettleByTxnRefAsync).
            if (transaction.IsFinalized && transaction.PaymentStatus != EPaymentStatus.Expired)
            {
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"IPN ignored because transaction {txnRef} is already {transaction.PaymentStatus}.");
                return new IpnResult(0, "Confirm Success");
            }

            // The library reports a bad checksum and a failed payment with the same shape
            // (IsSuccess == false), so the signature is verified explicitly to answer 97 instead of
            // silently recording a forged callback as a failed payment.
            if (!VnpaySignatureHelper.IsValid(query, _vnpaySettings.HashSecret))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"IPN rejected because the signature of transaction {txnRef} is invalid.");
                return new IpnResult(97, "Invalid signature");
            }

            if (!IsAmountMatching(query, transaction))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"IPN rejected because the amount of transaction {txnRef} does not match.");
                return new IpnResult(4, "Invalid amount");
            }

            var classification = ClassifySettlement(query, transaction);
            if (classification.IsInFlight)
            {
                // VNPAY has not finished with this transaction. Leaving the row pending and asking
                // VNPAY to call back again is what eventually settles it; freezing it now would
                // record a half-finished payment as final.
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"IPN for transaction {txnRef} is still in flight, VNPAY should retry.");
                return new IpnResult(99, "Unprocessed");
            }

            var affectedRows = await _paymentTransactionRepository.SettleByTxnRefAsync(
                txnRef, classification.Settlement!, cancellationToken);
            if (affectedRows == 0)
            {
                // Either a concurrent IPN settled the row first, or it had already expired and this
                // outcome is not a success, in which case the expiry stands.
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"IPN for transaction {txnRef} settled nothing, the stored result is kept.");
                return new IpnResult(0, "Confirm Success");
            }

            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"IPN processed for transaction {txnRef} with result {classification.Settlement!.Status}.");
            return new IpnResult(0, "Confirm Success");
        }

        #endregion

        #region GET

        public async Task<PaymentTransactionModel?> GetReturnResultAsync(IQueryCollection query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            var txnRef = QueryValue(query, "vnp_TxnRef");
            if (string.IsNullOrWhiteSpace(txnRef))
            {
                return null;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading payment transaction for VNPAY return url.");
            return await _paymentTransactionRepository.GetByTxnRefAsync(txnRef, cancellationToken);
        }

        public async Task<PaymentTransactionModel?> GetByIdAsync(Guid paymentTransactionId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transaction by id.");
            return await _paymentTransactionRepository.GetByIdAsync(paymentTransactionId, cancellationToken);
        }

        public async Task<PaymentTransactionModel?> GetByTxnRefAsync(string txnRef,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(txnRef))
            {
                throw new ArgumentException("Transaction reference is required.", nameof(txnRef));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transaction by txn ref.");
            return await _paymentTransactionRepository.GetByTxnRefAsync(txnRef, cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentTransactionModel>> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            if (invoiceId == Guid.Empty)
            {
                throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transactions by invoice id.");
            return await _paymentTransactionRepository.GetByInvoiceIdAsync(invoiceId, cancellationToken);
        }

        public async Task<PaymentTransactionModel?> GetInvoicePaymentStatusAsync(Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Resolving effective payment status by invoice id.");
            var transactions = await _paymentTransactionRepository.GetByInvoiceIdAsync(invoiceId,
                cancellationToken);
            return transactions.FirstOrDefault(transaction => transaction.IsSucceeded)
                   ?? transactions.FirstOrDefault();
        }

        public async Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingAsync(Guid? cursor,
            int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading payment transaction page.");
            return await _paymentTransactionRepository.GetApplyPagingAsync(cursor, pageSize,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingByAccountIdAsync(
            Guid? cursor, int pageSize, Guid accountId, CancellationToken cancellationToken = default)
        {
            if (accountId == Guid.Empty)
            {
                throw new ArgumentException("Account id is required.", nameof(accountId));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading payment transaction page by account id.");
            return await _paymentTransactionRepository.GetApplyPagingByAccountIdAsync(cursor, pageSize,
                accountId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingByStatusAsync(
            Guid? cursor, int pageSize, EPaymentStatus status,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading payment transaction page by status.");
            return await _paymentTransactionRepository.GetApplyPagingByStatusAsync(cursor, pageSize,
                status, cancellationToken);
        }

        #endregion

        #region Private

        /// <summary>
        /// Turns a verified IPN into the outcome to store. Returns <c>IsInFlight</c> when VNPAY has
        /// not finished, in which case nothing is written and VNPAY is asked to retry.
        /// </summary>
        private (bool IsInFlight, PaymentSettlementModel? Settlement) ClassifySettlement(
            IQueryCollection query, PaymentTransactionModel transaction)
        {
            PaymentResult result;
            try
            {
                result = _vnpay.GetPaymentResult(query);
            }
            catch (ArgumentException exception)
            {
                // The library throws when VNPAY omitted mandatory fields (vnp_BankCode among them).
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    $"Payment result of transaction {transaction.PaymentTxnRef} is incomplete.");
                throw;
            }

            // Keep the exact codes VNPAY sent ("00"); the enums only carry the numeric value.
            var responseCode = ResponseCode(query, "vnp_ResponseCode", (sbyte)result.PaymentResponse.Code);
            var transactionStatusCode = ResponseCode(query, "vnp_TransactionStatus",
                (sbyte)result.TransactionStatus.Code);

            // "01 - chưa hoàn tất" and "05 - VNPAY đang xử lý" are not outcomes, they are states on
            // the way to one.
            if (transactionStatusCode is IncompleteTransactionStatusCode or ProcessingTransactionStatusCode)
            {
                return (true, null);
            }

            var status = result.IsSuccess
                ? EPaymentStatus.Succeeded
                // "07 - debited but suspected fraud" and "04 - debited at the bank but not
                // successful at VNPAY" mean the customer may already have paid, so they are held
                // for reconciliation instead of being buried as an ordinary failure.
                : responseCode == DebitedFraudResponseCode
                  || transactionStatusCode == DebitedNotSettledTransactionStatusCode
                    ? EPaymentStatus.NeedsReview
                    : EPaymentStatus.Failed;

            var settlement = new PaymentSettlementModel(
                status,
                // Read the raw fields rather than the library's helpers: those substitute the
                // Vietnamese placeholder "Không xác định" for absent values, which would be stored
                // as if it were data.
                QueryValue(query, "vnp_TransactionNo"),
                QueryValue(query, "vnp_BankCode"),
                QueryValue(query, "vnp_BankTranNo"),
                responseCode,
                transactionStatusCode,
                QueryValue(query, "vnp_CardType"),
                result.Description ?? transaction.PaymentOrderInfo,
                // Only a success carries a real pay date. The library silently substitutes
                // DateTime.Now when vnp_PayDate is missing, so Timestamp is trusted only when the
                // field was actually present.
                result.IsSuccess && QueryValue(query, "vnp_PayDate") is not null
                    ? ToUtcFromGmt7(result.Timestamp)
                    : null);

            return (false, settlement);
        }

        private static bool IsAmountMatching(IQueryCollection query, PaymentTransactionModel transaction)
        {
            var amountText = QueryValue(query, "vnp_Amount");
            if (amountText is null)
            {
                return false;
            }

            return long.TryParse(amountText, NumberStyles.Integer, CultureInfo.InvariantCulture,
                       out var amountMinor)
                   && (decimal)amountMinor / 100m == transaction.PaymentAmount;
        }

        private static string? QueryValue(IQueryCollection query, string key)
        {
            var value = query[key].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static string ResponseCode(IQueryCollection query, string key, sbyte fallback)
        {
            return QueryValue(query, key) ?? fallback.ToString(CultureInfo.InvariantCulture);
        }

        private static DateTime? ToUtcFromGmt7(DateTime? vnpayTimestamp)
        {
            // vnp_PayDate is always GMT+7 (VNPAY spec) and is parsed as Unspecified by the library.
            // Store UTC so it lines up with the other columns (the DB session runs on UTC).
            return vnpayTimestamp is null
                ? null
                : DateTime.SpecifyKind(vnpayTimestamp.Value, DateTimeKind.Utc).AddHours(-7);
        }

        private static BankCode ResolveBankCode(string? bankCode)
        {
            return string.IsNullOrWhiteSpace(bankCode) || !Enum.TryParse<BankCode>(bankCode, true, out var parsed)
                ? BankCode.ANY
                : parsed;
        }

        private static DisplayLanguage ResolveLanguage(string? language)
        {
            return string.IsNullOrWhiteSpace(language)
                   || !Enum.TryParse<DisplayLanguage>(language, true, out var parsed)
                ? DisplayLanguage.Vietnamese
                : parsed;
        }

        #endregion
    }
}
