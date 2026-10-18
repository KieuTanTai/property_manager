using Contract.Interfaces.IApplication;
using Contract.Models.Contract;
using Contract.Presentation.Record.Violation;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging;
using Shared.Presentation.Controller;

namespace Contract.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViolationController(
        IViolationApplication violationApplication,
        ILogger<ViolationController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, "Contract", "Presentation/Controller")
    {
        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ViolationController> _logger = logger;

        private readonly IViolationApplication _violationApplication = violationApplication;

        #region POST

        [RequireHttps]
        [HttpPost]
        public async Task<IActionResult> AddViolationAsync(
            [FromBody] RecordViolationRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Violation creation request received.");
            try
            {
                var violationDate = request.ViolationDate ?? DateTime.Now;
                var violation = new ContractViolationModel(request.ContractId, request.ViolationContent);
                if (request.ViolationPenaltyAmount.HasValue)
                {
                    violation.SetViolationPenaltyAmount(request.ViolationPenaltyAmount);
                }
                violation.SetViolationDate(violationDate);
                violation.SetViolationDueDate(request.ViolationDueDate ?? violationDate.AddDays(7));

                var affectedRows = await _violationApplication.AddViolationAsync(violation, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Violation created: {violation.ViolationId}.");
                return affectedRows == 0 ? StatusCode(StatusCodes.Status500InternalServerError) : Ok(violation);
            }
            catch (OperationCanceledException ex)
            {
                return HandleCanceled(ex, "Violation creation request was canceled.");
            }
            catch (ArgumentException ex)
            {
                return HandleInvalidRequest(ex, "Violation creation request was rejected.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Violation creation request conflicted with the current application state.");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleFailure(ex, "Violation creation request failed.");
            }
        }

        #endregion

        #region PUT

        [RequireHttps]
        [HttpPut("{violationId:guid}")]
        public async Task<IActionResult> UpdateViolationAsync(
            [FromRoute] Guid violationId,
            [FromBody] RecordViolationUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Violation update request received for {violationId}.");
            if (violationId == Guid.Empty)
            {
                return BadRequest("Violation id is required.");
            }

            try
            {
                var violation = new ContractViolationModel(violationId, request.ContractId,
                    request.ViolationContent, request.ViolationPenaltyAmount,
                    request.ViolationDate, request.ViolationDueDate, request.ViolationIsResolved);
                var affectedRows = await _violationApplication.UpdateViolationAsync(violation, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Violation updated: {violationId}.");
                return affectedRows == 0 ? NotFound("Violation not found.") : Ok(violation);
            }
            catch (OperationCanceledException ex)
            {
                return HandleCanceled(ex, "Violation update request was canceled.");
            }
            catch (ArgumentException ex)
            {
                return HandleInvalidRequest(ex, "Violation update request was rejected.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Violation update request could not be applied to the current application state.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleFailure(ex, "Violation update request failed.");
            }
        }

        #endregion

        #region DELETE

        [RequireHttps]
        [HttpDelete("{violationId:guid}")]
        public async Task<IActionResult> DeleteViolationAsync(
            [FromRoute] Guid violationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Violation deletion request received for {violationId}.");
            try
            {
                var affectedRows = await _violationApplication.DeleteViolationAsync(violationId, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Violation deleted: {violationId}.");
                return affectedRows == 0 ? NotFound("Violation not found.") : NoContent();
            }
            catch (OperationCanceledException ex)
            {
                return HandleCanceled(ex, "Violation deletion request was canceled.");
            }
            catch (ArgumentException ex)
            {
                return HandleInvalidRequest(ex, "Violation deletion request was rejected.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Violation deletion request conflicted with the current application state.");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleFailure(ex, "Violation deletion request failed.");
            }
        }

        #endregion

        #region GET

        [RequireHttps]
        [HttpGet]
        public async Task<IActionResult> GetViolationsAsync(
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Violation list request received.");
            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than 0.");
            }

            try
            {
                var result = await _violationApplication.GetApplyPagingAsync(cursor, pageSize, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Violation list request completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No violations found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return HandleCanceled(ex, "Violation list request was canceled.");
            }
            catch (ArgumentException ex)
            {
                return HandleInvalidRequest(ex, "Violation list request was rejected.");
            }
            catch (Exception ex)
            {
                return HandleFailure(ex, "Violation list request failed.");
            }
        }

        [RequireHttps]
        [HttpGet("by-contract/{contractId:guid}")]
        public async Task<IActionResult> GetViolationsByContractIdAsync(
            [FromRoute] Guid contractId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Violation lookup request received for contract {contractId}.");
            try
            {
                var result = await _violationApplication.GetViolationsByContractIdAsync(contractId, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Violation lookup completed for contract {contractId}: {result.Count} records.");
                return result.Count == 0 ? NotFound("No violations found for this contract.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return HandleCanceled(ex, "Violation lookup request was canceled.");
            }
            catch (ArgumentException ex)
            {
                return HandleInvalidRequest(ex, "Violation lookup request was rejected.");
            }
            catch (Exception ex)
            {
                return HandleFailure(ex, "Violation lookup request failed.");
            }
        }

        [RequireHttps]
        [HttpGet("by-content")]
        public async Task<IActionResult> GetViolationsByContentAsync(
            [FromQuery] string violationContent,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Violation content search received.");
            try
            {
                var result = await _violationApplication.GetApplyPagingByViolationContentAsync(
                    cursor, pageSize, violationContent, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Violation content search completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No violations found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return HandleCanceled(ex, "Violation content search was canceled.");
            }
            catch (ArgumentException ex)
            {
                return HandleInvalidRequest(ex, "Violation content search was rejected.");
            }
            catch (Exception ex)
            {
                return HandleFailure(ex, "Violation content search failed.");
            }
        }

        [RequireHttps]
        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetViolationsByDateRangeAsync(
            [FromQuery] DateTime minViolationDate,
            [FromQuery] DateTime maxViolationDate,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Violation date range search received.");
            try
            {
                var result = await _violationApplication.GetApplyPagingByViolationDateRangeAsync(
                    cursor, pageSize, minViolationDate, maxViolationDate, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Violation date range search completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No violations found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return HandleCanceled(ex, "Violation date range search was canceled.");
            }
            catch (ArgumentException ex)
            {
                return HandleInvalidRequest(ex, "Violation date range search was rejected.");
            }
            catch (Exception ex)
            {
                return HandleFailure(ex, "Violation date range search failed.");
            }
        }

        [RequireHttps]
        [HttpGet("status/{isResolved:bool}")]
        public async Task<IActionResult> GetViolationsByStatusAsync(
            [FromRoute] bool isResolved,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Violation status request received for resolved={isResolved}.");
            try
            {
                var result = await _violationApplication.GetApplyPagingByViolationStatusAsync(
                    cursor, pageSize, isResolved, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Violation status request completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No violations found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return HandleCanceled(ex, "Violation status request was canceled.");
            }
            catch (ArgumentException ex)
            {
                return HandleInvalidRequest(ex, "Violation status request was rejected.");
            }
            catch (Exception ex)
            {
                return HandleFailure(ex, "Violation status request failed.");
            }
        }

        #endregion

        #region PRIVATE

        protected virtual IActionResult HandleCanceled(OperationCanceledException exception, string message)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, message);
            return BadRequest($"Request canceled. {exception.Message}");
        }

        protected virtual IActionResult HandleInvalidRequest(ArgumentException exception, string message)
        {
            _logger.LogLayerWarning(_logPool, Module, Layer, message);
            return BadRequest(exception.Message);
        }

        protected virtual IActionResult HandleFailure(Exception exception, string message)
        {
            _logger.LogLayerError(_logPool, Module, Layer, exception, message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An unexpected error occurred while processing the request.");
        }

        #endregion
    }
}