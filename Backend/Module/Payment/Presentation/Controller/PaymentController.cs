using Microsoft.AspNetCore.Mvc;
using Payment.Interfaces.IApplication;
using Payment.Models.Payment;
using Payment.Presentation.Record.Payment;
using Payment.Utils.Enum;
using Shared.Logging;

namespace Payment.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(
        IPaymentApplication paymentApplication,
        ILogger<PaymentController> logger,
        ILogPool logPool) : ControllerBase
    {
        private const string Module = "Payment";

        private const string Layer = "Presentation/Controller";

        private readonly IPaymentApplication _paymentApplication = paymentApplication;

        private readonly ILogger<PaymentController> _logger = logger;

        private readonly ILogPool _logPool = logPool;

        #region POST

        [RequireHttps]
        [HttpPost("create-url")]
        public async Task<IActionResult> CreatePaymentUrlAsync(
            [FromBody] RecordCreatePaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.InvoiceId == Guid.Empty || request.AccountId == Guid.Empty)
            {
                return BadRequest("Invoice id and account id are required.");
            }

            try
            {
                var result = await _paymentApplication.CreatePaymentUrlAsync(
                    request.InvoiceId,
                    request.AccountId,
                    request.Amount,
                    request.Description,
                    request.BankCode,
                    request.Language,
                    ResolveClientIpAddress(),
                    cancellationToken);

                return Ok(MapResponse(result.Transaction, result.PaymentUrl));
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Payment url creation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region VNPAY callbacks

        /// <summary>
        /// VNPAY IPN (server to server). No [RequireHttps] on purpose because the request is
        /// usually forwarded by a TLS-terminating tunnel. Must always answer with the VNPAY
        /// acknowledgement body and must never throw.
        /// </summary>
        [HttpGet("ipn")]
        public async Task<IActionResult> ProcessIpnAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _paymentApplication.ProcessIpnAsync(Request.Query, cancellationToken);
                return Ok(new { RspCode = result.RspCode.ToString("D2"), Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "IPN processing failed.");
                return Ok(new { RspCode = "99", Message = "Unprocessed" });
            }
        }

        /// <summary>
        /// Browser redirect target after payment. Read-only: reports the stored transaction so the
        /// caller can render the outcome. Never settle here, IPN is the source of truth.
        /// </summary>
        [HttpGet("return")]
        public async Task<IActionResult> GetReturnResultAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _paymentApplication.GetReturnResultAsync(Request.Query,
                    cancellationToken);
                return transaction is null
                    ? NotFound("Payment transaction not found.")
                    : Ok(MapResponse(transaction));
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Payment return handling failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region GET

        [RequireHttps]
        [HttpGet("{paymentTransactionId:guid}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] Guid paymentTransactionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _paymentApplication.GetByIdAsync(paymentTransactionId,
                    cancellationToken);
                return transaction is null
                    ? NotFound("Payment transaction not found.")
                    : Ok(MapResponse(transaction));
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Payment transaction lookup failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-txn-ref/{txnRef}")]
        public async Task<IActionResult> GetByTxnRefAsync(
            [FromRoute] string txnRef,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(txnRef))
            {
                return BadRequest("Transaction reference is required.");
            }

            try
            {
                var transaction = await _paymentApplication.GetByTxnRefAsync(txnRef, cancellationToken);
                return transaction is null
                    ? NotFound("Payment transaction not found.")
                    : Ok(MapResponse(transaction));
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Payment transaction lookup failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-invoice/{invoiceId:guid}")]
        public async Task<IActionResult> GetByInvoiceIdAsync(
            [FromRoute] Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _paymentApplication.GetByInvoiceIdAsync(invoiceId,
                    cancellationToken);
                return transactions.Count == 0
                    ? NotFound("No payment transaction found for the invoice.")
                    : Ok(transactions.Select(transaction => MapResponse(transaction)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Payment transaction lookup failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-invoice/{invoiceId:guid}/status")]
        public async Task<IActionResult> GetInvoicePaymentStatusAsync(
            [FromRoute] Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _paymentApplication.GetInvoicePaymentStatusAsync(invoiceId,
                    cancellationToken);
                return Ok(new RecordInvoicePaymentStatusResponse(
                    invoiceId,
                    transaction?.IsSucceeded ?? false,
                    transaction?.PaymentTransactionId,
                    transaction?.PaymentTxnRef,
                    transaction?.PaymentStatus,
                    transaction?.PaymentAmount,
                    transaction?.PaymentPayDate));
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Invoice payment status lookup failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-account/{accountId:guid}")]
        public async Task<IActionResult> GetApplyPagingByAccountIdAsync(
            [FromRoute] Guid accountId,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than 0.");
            }

            try
            {
                var result = await _paymentApplication.GetApplyPagingByAccountIdAsync(cursor, pageSize,
                    accountId, cancellationToken);
                return result.Items.Count == 0
                    ? NotFound("No payment transaction found for the account.")
                    : Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Payment transaction list failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetApplyPagingByStatusAsync(
            [FromRoute] EPaymentStatus status,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than 0.");
            }

            try
            {
                var result = await _paymentApplication.GetApplyPagingByStatusAsync(cursor, pageSize,
                    status, cancellationToken);
                return result.Items.Count == 0
                    ? NotFound("No payment transaction found for the status.")
                    : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Payment transaction list failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region Private

        private string ResolveClientIpAddress()
        {
            // Deliberately not VNPAY.NET's NetworkHelper: it reverse-resolves the address with
            // Dns.GetHostEntry for anything IPv6, which throws on IPv4-mapped addresses (the
            // normal case in a container or behind a proxy) and NREs when the address is null.
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress is null)
            {
                // Only reachable in exotic hosting setups; vnp_IpAddr is mandatory.
                return "127.0.0.1";
            }

            // vnp_IpAddr expects IPv4, so unwrap ::ffff:1.2.3.4 style addresses.
            return remoteIpAddress.IsIPv4MappedToIPv6
                ? remoteIpAddress.MapToIPv4().ToString()
                : remoteIpAddress.ToString();
        }

        private static RecordPaymentResponse MapResponse(PaymentTransactionModel transaction,
            string? paymentUrl = null)
        {
            return new RecordPaymentResponse(
                transaction.PaymentTransactionId,
                transaction.PaymentInvoiceId,
                transaction.PaymentAccountId,
                transaction.PaymentAmount,
                transaction.PaymentCurrency,
                transaction.PaymentStatus,
                transaction.PaymentTxnRef,
                paymentUrl,
                transaction.PaymentGatewayTransactionNumber,
                transaction.PaymentBankCode,
                transaction.PaymentBankTranNo,
                transaction.PaymentResponseCode,
                transaction.PaymentTransactionStatusCode,
                transaction.PaymentCardType,
                transaction.PaymentOrderInfo,
                transaction.PaymentPayDate,
                transaction.PaymentExpiresAt,
                transaction.PaymentCreatedAt,
                transaction.PaymentUpdatedAt);
        }

        #endregion
    }
}
