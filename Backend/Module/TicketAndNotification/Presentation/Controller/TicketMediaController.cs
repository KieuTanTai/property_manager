using Microsoft.AspNetCore.Mvc;
using Shared.Logging;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Models.Ticket;
using TicketAndNotification.Presentation.Record.TicketMedia;

namespace TicketAndNotification.Presentation.Controller
{
    [ApiController]
    [Route("api/ticket-medias")]
    public class TicketMediaController(
        ITicketMediaApplication ticketMediaApplication,
        ILogger<TicketMediaController> logger,
        ILogPool logPool) : ControllerBase
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Presentation/Controller";

        private readonly ITicketMediaApplication _ticketMediaApplication = ticketMediaApplication;
        private readonly ILogger<TicketMediaController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _ticketMediaApplication.GetAllAsync(cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket media list request failed.");
            }
        }

        [HttpGet("{ticketMediaId:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int ticketMediaId,
            CancellationToken cancellationToken = default)
        {
            if (ticketMediaId <= 0)
            {
                return BadRequest("A valid ticket media id is required.");
            }

            try
            {
                var ticketMedia = await _ticketMediaApplication.GetByIdAsync(
                    ticketMediaId, cancellationToken);
                return ticketMedia is null
                    ? NotFound("Ticket media was not found.")
                    : Ok(ticketMedia);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket media lookup request failed.");
            }
        }

        [HttpGet("by-ids")]
        public async Task<IActionResult> GetByIdsAsync(
            [FromQuery] List<int> ticketMediaIds,
            CancellationToken cancellationToken = default)
        {
            if (ticketMediaIds.Count == 0 || ticketMediaIds.Any(id => id <= 0))
            {
                return BadRequest("At least one valid ticket media id is required.");
            }

            try
            {
                return Ok(await _ticketMediaApplication.GetByIdsAsync(
                    ticketMediaIds, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket media lookup by ids failed.");
            }
        }

        [HttpGet("by-ticket/{ticketId:guid}")]
        public async Task<IActionResult> GetByTicketIdAsync(
            [FromRoute] Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            if (ticketId == Guid.Empty)
            {
                return BadRequest("A valid ticket id is required.");
            }

            try
            {
                return Ok(await _ticketMediaApplication.GetByTicketIdAsync(
                    ticketId, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket media ticket lookup failed.");
            }
        }

        #endregion

        #region POST

        [HttpPost]
        public async Task<IActionResult> AddAsync(
            [FromBody] CreateTicketMediaRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var ticketMedia = new TicketMediaModel(request.TicketId, request.ImageUrl);
                var result = await _ticketMediaApplication.AddAsync(
                    ticketMedia, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync),
                    new { ticketMediaId = result.TicketMediaId }, result);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket media creation request failed.");
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> AddRangeAsync(
            [FromBody] List<CreateTicketMediaRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return BadRequest("Ticket media collection cannot be empty.");
            }

            try
            {
                var ticketMedias = requests.Select(request =>
                    new TicketMediaModel(request.TicketId, request.ImageUrl));
                var count = await _ticketMediaApplication.AddRangeAsync(
                    ticketMedias, cancellationToken);
                return Ok(new { affectedRows = count });
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket media batch creation failed.");
            }
        }

        #endregion

        #region PUT

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(
            [FromBody] UpdateTicketMediaRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var ticketMedia = new TicketMediaModel(
                    request.TicketMediaId, request.TicketId, request.ImageUrl);
                return Ok(await _ticketMediaApplication.UpdateAsync(
                    ticketMedia, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket media update request failed.");
            }
        }

        [HttpPut("batch")]
        public async Task<IActionResult> UpdateRangeAsync(
            [FromBody] List<UpdateTicketMediaRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return BadRequest("Ticket media collection cannot be empty.");
            }

            try
            {
                var ticketMedias = requests.Select(request =>
                    new TicketMediaModel(
                        request.TicketMediaId, request.TicketId, request.ImageUrl));
                var count = await _ticketMediaApplication.UpdateRangeAsync(
                    ticketMedias, cancellationToken);
                return Ok(new { affectedRows = count });
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket media batch update failed.");
            }
        }

        #endregion

        private IActionResult HandleError(Exception exception, string message)
        {
            if (exception is OperationCanceledException)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Ticket media request was canceled.");
                return BadRequest("Request was canceled.");
            }

            _logger.LogLayerError(_logPool, Module, Layer, exception, message);
            return BadRequest(exception.Message);
        }
    }
}
