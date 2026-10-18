using Contract.Interfaces.IApplication;
using Contract.Models.Invoice;
using Contract.Presentation.Record.Receipt;
using Microsoft.AspNetCore.Mvc;
using Shared.Enum;
using Shared.Logging;
using Shared.Presentation.Controller;

namespace Contract.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptController(
        IReceiptApplication receiptApplication,
        ILogger<ReceiptController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, "Contract", "Presentation/Controller")
    {
        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ReceiptController> _logger = logger;

        private readonly IReceiptApplication _receiptApplication = receiptApplication;

        #region POST

        [RequireHttps]
        [HttpPost]
        public async Task<IActionResult> AddReceiptAsync(
            [FromBody] RecordReceiptRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Receipt creation request received.");
            try
            {
                var receipt = CreateReceipt(request);
                var result = await _receiptApplication.AddReceiptAsync(receipt, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipt created: {result.ReceiptId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt creation request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt creation request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region PUT

        [RequireHttps]
        [HttpPut("{receiptId:guid}/invoice/{invoiceId:guid}")]
        public async Task<IActionResult> UpdateReceiptAsync(
            [FromRoute] Guid receiptId,
            [FromRoute] Guid invoiceId,
            [FromBody] RecordReceiptUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Receipt update request received for {receiptId}.");
            try
            {
                var result = await _receiptApplication.UpdateReceiptAsync(receiptId, invoiceId,
                    request.PaymentDate, request.Amount, request.TransactionInfo, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipt updated: {result.ReceiptId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt update request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Receipt update request was rejected.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt update request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region PRIVATE

        private static ReceiptModel CreateReceipt(RecordReceiptRequest request)
        {
            var receipt = new ReceiptModel(request.InvoiceId, request.CreatedByAccountId,
                request.Amount);
            receipt.SetPaymentMethod(request.PaymentMethod);
            receipt.SetPaymentDate(request.PaymentDate);
            receipt.SetTransactionReference(request.TransactionReference);
            receipt.SetGatewayTransactionNumber(request.GatewayTransactionNumber);
            receipt.SetTransactionInfo(request.TransactionInfo);
            receipt.SetBankName(request.BankName);
            return receipt;
        }

        #endregion

        #region GET

        [RequireHttps]
        [HttpGet]
        public async Task<IActionResult> GetReceiptsAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Receipt list request received.");
            try
            {
                var result = await _receiptApplication.GetAllReceiptsAsync(cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipt list request completed: {result.Count} records.");
                return result.Count == 0 ? NotFound("No receipts found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt list request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt list request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("{receiptId:guid}")]
        public async Task<IActionResult> GetReceiptByIdAsync(
            [FromRoute] Guid receiptId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Receipt lookup request received for {receiptId}.");
            try
            {
                var result = await _receiptApplication.GetByReceiptIdAsync(receiptId, cancellationToken);
                if (result is null)
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer,
                        $"Receipt {receiptId} was not found.");
                    return NotFound("Receipt not found.");
                }
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipt loaded: {result.ReceiptId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt lookup request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt lookup request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("{receiptId:guid}/exists")]
        public async Task<IActionResult> GetReceiptExistsAsync(
            [FromRoute] Guid receiptId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Receipt existence request received for {receiptId}.");
            try
            {
                var result = await _receiptApplication.ExistsAsync(receiptId, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipt existence request completed for {receiptId}: {result}.");
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt existence request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt existence request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-ids")]
        public async Task<IActionResult> GetReceiptsByIdsAsync(
            [FromQuery] Guid[] ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Receipt lookup by ids request received.");
            try
            {
                var result = await _receiptApplication.GetReceiptsByIdsAsync(ids, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipt lookup by ids completed: {result.Count} records.");
                return result.Count == 0 ? NotFound("No receipts found.") : Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt lookup by ids request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt lookup by ids request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-invoice/{invoiceId:guid}")]
        public async Task<IActionResult> GetReceiptByInvoiceIdAsync(
            [FromRoute] Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Receipt by invoice request received for {invoiceId}.");
            try
            {
                var result = await _receiptApplication.GetReceiptByInvoiceIdAsync(invoiceId, cancellationToken);
                if (result is null)
                {
                    return NotFound("Receipt not found.");
                }
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipt loaded for invoice {invoiceId}: {result.ReceiptId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt by invoice request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt by invoice request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-account/{accountId:guid}")]
        public async Task<IActionResult> GetReceiptsByAccountIdAsync(
            [FromRoute] Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Receipt by account request received for {accountId}.");
            try
            {
                var result = await _receiptApplication.GetReceiptsByAccountIdAsync(accountId, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipts loaded for account {accountId}: {result.Count} records.");
                return result.Count == 0 ? NotFound("No receipts found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt by account request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt by account request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-payment-method/{paymentMethod}")]
        public async Task<IActionResult> GetReceiptsByPaymentMethodAsync(
            [FromRoute] EReceiptPaymentMethod paymentMethod,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Receipt payment method request received for {paymentMethod}.");
            if (!Enum.IsDefined(paymentMethod))
            {
                return BadRequest("Payment method is invalid.");
            }
            try
            {
                var result = await _receiptApplication.GetReceiptsByPaymentMethodAsync(
                    paymentMethod, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipts loaded by payment method {paymentMethod}: {result.Count} records.");
                return result.Count == 0 ? NotFound("No receipts found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt payment method request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt payment method request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-payment-date")]
        public async Task<IActionResult> GetReceiptsByPaymentDateAsync(
            [FromQuery] DateTime paymentDate,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Receipt payment date request received for {paymentDate:yyyy-MM-dd}.");
            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than 0.");
            }
            try
            {
                var result = await _receiptApplication.GetApplyPagingByReceiptPaymentDateAsync(
                    cursor, pageSize, paymentDate, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Receipt payment date request completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No receipts found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Receipt payment date request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Receipt payment date request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion
    }
}