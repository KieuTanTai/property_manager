using Contract.Interfaces.IApplication;
using Contract.Models.Invoice;
using Contract.Presentation.Record.Invoice;
using Contract.Utils.Enum;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging;
using Shared.Presentation.Controller;

namespace Contract.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController(
        IInvoiceApplication invoiceApplication,
        ILogger<InvoiceController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, "Contract", "Presentation/Controller")
    {
        private readonly IInvoiceApplication _invoiceApplication = invoiceApplication;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<InvoiceController> _logger = logger;

        #region POST

        [RequireHttps]
        [HttpPost]
        public async Task<IActionResult> AddInvoiceAsync(
            [FromBody] RecordInvoiceRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Invoice creation request received.");
            try
            {
                var invoice = new MonthlyInvoiceModel(request.InvoiceId, request.ContractId,
                    request.PaymentDate, request.DueDate, request.TotalAmount, request.Status);
                var details = request.Details.Select(detail => CreateDetail(request.InvoiceId, detail)).ToList();
                var result = await _invoiceApplication.AddInvoiceAsync(invoice, details, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Invoice created: {result.InvoiceId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Invoice creation request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Invoice creation request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region PUT

        [RequireHttps]
        [HttpPut("{invoiceId:guid}")]
        public async Task<IActionResult> UpdateInvoiceAsync(
            [FromRoute] Guid invoiceId,
            [FromBody] RecordInvoiceUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Invoice update request received for {invoiceId}.");
            if (invoiceId == Guid.Empty)
            {
                return BadRequest("Invoice id is required.");
            }
            if (!Enum.IsDefined(request.Status))
            {
                return BadRequest("Invoice status is invalid.");
            }

            try
            {
                var invoice = new MonthlyInvoiceModel(invoiceId, request.ContractId,
                    request.PaymentDate, request.DueDate, request.TotalAmount, request.Status);
                var details = request.Details?.Select(detail => CreateDetail(invoiceId, detail)).ToList();
                var result = await _invoiceApplication.UpdateInvoiceAsync(invoice, details, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Invoice updated: {result.InvoiceId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Invoice update request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Invoice update request was rejected.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Invoice update request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region PRIVATE

        private static InvoiceDetailModel CreateDetail(Guid invoiceId,
            RecordInvoiceDetailRequest request)
        {
            return new InvoiceDetailModel(request.DetailId ?? 0, invoiceId,
                request.PremiseId, request.RentalPrice, request.ElectricityFee,
                request.WaterFee, request.GarbageFee);
        }

        #endregion

        #region GET

        [RequireHttps]
        [HttpGet]
        public async Task<IActionResult> GetInvoicesAsync(
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool isGetDetails = true,
            [FromQuery] bool isGetReceipt = true,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Invoice list request received.");
            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than 0.");
            }

            try
            {
                var result = await _invoiceApplication.GetApplyPagingAsync(
                    cursor, pageSize, isGetDetails, isGetReceipt, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Invoice list request completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No invoices found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Invoice list request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Invoice list request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("{invoiceId:guid}")]
        public async Task<IActionResult> GetInvoiceByIdAsync(
            [FromRoute] Guid invoiceId,
            [FromQuery] bool isGetDetails = true,
            [FromQuery] bool isGetReceipt = true,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Invoice lookup request received for {invoiceId}.");
            try
            {
                var result = await _invoiceApplication.GetByInvoiceIdAsync(
                    invoiceId, isGetDetails, isGetReceipt, cancellationToken);
                if (result is null)
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer,
                        $"Invoice {invoiceId} was not found.");
                    return NotFound("Invoice not found.");
                }
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Invoice loaded: {result.InvoiceId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Invoice lookup request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Invoice lookup request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetInvoicesByStatusAsync(
            [FromRoute] EInvoiceStatus status,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool isGetDetails = true,
            [FromQuery] bool isGetReceipt = true,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Invoice status request received for {status}.");
            if (!Enum.IsDefined(status))
            {
                return BadRequest("Invoice status is invalid.");
            }
            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than 0.");
            }

            try
            {
                var result = await _invoiceApplication.GetApplyPagingByInvoiceStatusAsync(
                    cursor, pageSize, status, isGetDetails, isGetReceipt, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Invoice status request completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No invoices found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Invoice status request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Invoice status request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion
    }
}