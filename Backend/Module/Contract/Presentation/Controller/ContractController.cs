using Contract.Interfaces.IApplication;
using Contract.Models.Contract;
using Contract.Presentation.Record.Contract;
using Contract.Utils.Enum;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging;

namespace Contract.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractController(
        IContractApplication contractApplication,
        ILogger<ContractController> logger,
        ILogPool logPool) : ControllerBase
    {
        private const string Module = "Contract";
        private const string Layer = "Presentation/Controller";

        private readonly IContractApplication _contractApplication = contractApplication;
        private readonly ILogger<ContractController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        [RequireHttps]
        [HttpGet]
        public async Task<IActionResult> GetContractsAsync(
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool isGetViolations = false,
            [FromQuery] bool isGetInvoices = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Contract list request received.");
            if (pageSize <= 0)
                return BadRequest("Page size must be greater than 0.");

            try
            {
                var result = await _contractApplication.GetApplyPagingAsync(
                    cursor, pageSize, isGetViolations, isGetInvoices, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Contract list request completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No contracts found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Contract list request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contract list request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("{contractId:guid}")]
        public async Task<IActionResult> GetContractByIdAsync(
            [FromRoute] Guid contractId,
            [FromQuery] bool isGetViolations = false,
            [FromQuery] bool isGetInvoices = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Contract lookup request received for {contractId}.");
            try
            {
                var result = await _contractApplication.GetByIdAsync(
                    contractId, isGetViolations, isGetInvoices, cancellationToken);
                if (result is null)
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer,
                        $"Contract {contractId} was not found.");
                    return NotFound("Contract not found.");
                }
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Contract loaded: {result.ContractId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Contract lookup request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contract lookup request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetContractsByStatusAsync(
            [FromRoute] EContractStatus status,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool isGetViolations = false,
            [FromQuery] bool isGetInvoices = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Contract status request received for {status}.");
            if (!Enum.IsDefined(status))
                return BadRequest("Contract status is invalid.");
            if (pageSize <= 0)
                return BadRequest("Page size must be greater than 0.");

            try
            {
                var result = await _contractApplication.GetApplyPagingByStatusAsync(
                    cursor, pageSize, status, isGetViolations, isGetInvoices, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Contract status request completed: {result.Items.Count} records.");
                return result.Items.Count == 0 ? NotFound("No contracts found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Contract status request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Contract status request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region POST

        [RequireHttps]
        [HttpPost]
        public async Task<IActionResult> AddContractAsync(
            [FromBody] RecordContractRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Contract creation request received.");
            try
            {
                var contract = new ContractModel(request.AccountId, request.Deposit,
                    request.RentalPrice, request.PremiseReturnDate);
                var result = await _contractApplication.AddAsync(
                    contract, request.RegulationIds, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Contract created: {result.ContractId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Contract creation request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Contract creation request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region PUT

        [RequireHttps]
        [HttpPut("{contractId:guid}")]
        public async Task<IActionResult> UpdateContractAsync(
            [FromRoute] Guid contractId,
            [FromBody] RecordContractUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                $"Contract update request received for {contractId}.");
            if (contractId == Guid.Empty)
                return BadRequest("Contract id is required.");
            if (!Enum.IsDefined(request.Status))
                return BadRequest("Contract status is invalid.");

            try
            {
                var contract = new ContractModel(contractId, request.AccountId,
                    request.Deposit, request.RentalPrice, request.PremiseReturnDate,
                    request.Status, request.TerminationDate);
                var result = await _contractApplication.UpdateAsync(
                    contract, request.RegulationIds, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Contract updated: {result.ContractId}.");
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Contract update request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Contract update request was rejected.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Contract update request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

    }
}
