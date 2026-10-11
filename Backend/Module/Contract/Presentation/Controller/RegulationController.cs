using Contract.Interfaces.IApplication;
using Contract.Models.Regulation;
using Contract.Presentation.Record.Regulation;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging;

namespace Contract.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegulationController(
        IRegulationApplication regulationApplication,
        ILogger<RegulationController> logger,
        ILogPool logPool) : ControllerBase
    {
        private const string Module = "Contract";
        private const string Layer = "Presentation/Controller";

        private readonly IRegulationApplication _regulationApplication = regulationApplication;
        private readonly ILogger<RegulationController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        [RequireHttps]
        [HttpGet]
        public async Task<IActionResult> GetRegulationsAsync(
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
                var result = await _regulationApplication.GetApplyPagingAsync(cursor, pageSize, cancellationToken);
                return result.Items.Count == 0 ? NotFound("No regulations found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Regulation list request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("{regulationId:guid}")]
        public async Task<IActionResult> GetRegulationByIdAsync(
            [FromRoute] Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var regulation = await _regulationApplication.GetByIdAsync(regulationId, cancellationToken);
                return regulation is null ? NotFound("Regulation not found.") : Ok(regulation);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Regulation lookup request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("by-name")]
        public async Task<IActionResult> GetRegulationByNameAsync(
            [FromQuery] string regulationName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(regulationName))
            {
                return BadRequest("Regulation name is required.");
            }

            try
            {
                return Ok(await _regulationApplication.GetByNameAsync(regulationName, cancellationToken));
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Regulation lookup request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("status/{isActive:bool}")]
        public async Task<IActionResult> GetRegulationsByStatusAsync(
            [FromRoute] bool isActive,
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
                var result = await _regulationApplication.GetApplyPagingByStatusAsync(
                    cursor, pageSize, isActive, cancellationToken);
                return result.Items.Count == 0 ? NotFound("No regulations found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Regulation status request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region POST

        [RequireHttps]
        [HttpPost]
        public async Task<IActionResult> AddRegulationAsync(
            [FromBody] RecordRegulationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var regulation = new RegulationModel(request.RegulationName);
                regulation.SetRegulationDescription(request.RegulationDescription);
                regulation.SetRegulationFineAmount(request.RegulationFineAmount);
                regulation.SetRegulationIsActive(request.RegulationIsActive);
                var created = await _regulationApplication.AddAsync(regulation, cancellationToken); 
                return Ok(created);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Regulation creation request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region PUT

        [RequireHttps]
        [HttpPut("{regulationId:guid}")]
        public async Task<IActionResult> UpdateRegulationAsync(
            [FromRoute] Guid regulationId,
            [FromBody] RecordRegulationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (regulationId == Guid.Empty)
            {
                return BadRequest("Regulation id is required.");
            }

            try
            {
                var regulation = new RegulationModel(regulationId, request.RegulationName,
                    request.RegulationDescription, request.RegulationFineAmount, request.RegulationIsActive);
                var affectedRows = await _regulationApplication.UpdateAsync(regulation, cancellationToken);
                return affectedRows == 0 ? BadRequest("Could not update regulation.") : Ok(regulation);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Regulation update request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region DELETE

        [RequireHttps]
        [HttpDelete("{regulationId:guid}")]
        public async Task<IActionResult> SoftDeleteRegulationAsync(
            [FromRoute] Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var affectedRows = await _regulationApplication.SoftDeleteAsync(regulationId, cancellationToken);
                return affectedRows == 0 ? BadRequest("Could not delete regulation.") : NoContent();
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Regulation deletion request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

    }
}
