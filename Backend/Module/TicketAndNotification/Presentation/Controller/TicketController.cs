using Microsoft.AspNetCore.Mvc;
using Shared.Enum;
using Shared.Logging;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Models.Ticket;
using TicketAndNotification.Presentation.Record.Ticket;

namespace TicketAndNotification.Presentation.Controller
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketController(
        ITicketApplication ticketApplication,
        ILogger<TicketController> logger,
        ILogPool logPool) : ControllerBase
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Presentation/Controller";

        private readonly ITicketApplication _ticketApplication = ticketApplication;
        private readonly ILogger<TicketController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Ticket list request received.");
            try
            {
                return Ok(await _ticketApplication.GetAllAsync(cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket list request failed.");
            }
        }

        [HttpGet("{ticketId:guid}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Ticket lookup request received.");
            try
            {
                var ticket = await _ticketApplication.GetByIdAsync(ticketId, cancellationToken);
                return ticket is null ? NotFound("Ticket was not found.") : Ok(ticket);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket lookup request failed.");
            }
        }

        [HttpGet("by-ids")]
        public async Task<IActionResult> GetByIdsAsync(
            [FromQuery] List<Guid> ticketIds,
            CancellationToken cancellationToken = default)
        {
            if (ticketIds.Count == 0 || ticketIds.Contains(Guid.Empty))
            {
                return BadRequest("At least one valid ticket id is required.");
            }

            try
            {
                return Ok(await _ticketApplication.GetByIdsAsync(ticketIds, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket lookup by ids failed.");
            }
        }

        [HttpGet("by-account/{accountId:guid}")]
        public async Task<IActionResult> GetByAccountIdAsync(
            [FromRoute] Guid accountId,
            CancellationToken cancellationToken = default)
        {
            if (accountId == Guid.Empty)
            {
                return BadRequest("A valid account id is required.");
            }

            try
            {
                return Ok(await _ticketApplication.GetByAccountIdAsync(
                    accountId, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket account lookup failed.");
            }
        }

        [HttpGet("by-type/{type}")]
        public async Task<IActionResult> GetByTypeAsync(
            [FromRoute] ETicketType type,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(type))
            {
                return BadRequest("Ticket type is invalid.");
            }

            try
            {
                return Ok(await _ticketApplication.GetByTypeAsync(type, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket type lookup failed.");
            }
        }

        [HttpGet("by-resolution/{isResolved:bool}")]
        public async Task<IActionResult> GetByResolutionAsync(
            [FromRoute] bool isResolved,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _ticketApplication.GetByIsResolvedAsync(
                    isResolved, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket resolution lookup failed.");
            }
        }

        [HttpGet("{ticketId:guid}/content")]
        public async Task<IActionResult> GetContentByIdAsync(
            [FromRoute] Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var content = await _ticketApplication.GetContentByIdAsync(
                    ticketId, cancellationToken);
                return content is null ? NotFound("Ticket was not found.") : Ok(content);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket content lookup failed.");
            }
        }

        #endregion

        #region POST

        [HttpPost]
        public async Task<IActionResult> AddAsync(
            [FromBody] CreateTicketRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var ticket = new TicketModel(request.AccountId, request.Content, request.Type);
                var result = await _ticketApplication.AddAsync(ticket, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync),
                    new { ticketId = result.TicketId }, result);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket creation request failed.");
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> AddRangeAsync(
            [FromBody] List<CreateTicketRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return BadRequest("Ticket collection cannot be empty.");
            }

            try
            {
                var tickets = requests.Select(request =>
                    new TicketModel(request.AccountId, request.Content, request.Type));
                var count = await _ticketApplication.AddRangeAsync(tickets, cancellationToken);
                return Ok(new { affectedRows = count });
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket batch creation request failed.");
            }
        }

        #endregion

        #region PUT

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(
            [FromBody] UpdateTicketRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var ticket = new TicketModel(
                    request.TicketId,
                    request.AccountId,
                    request.Content,
                    request.Type,
                    request.IsResolved);
                return Ok(await _ticketApplication.UpdateAsync(ticket, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket update request failed.");
            }
        }

        [HttpPut("batch")]
        public async Task<IActionResult> UpdateRangeAsync(
            [FromBody] List<UpdateTicketRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return BadRequest("Ticket collection cannot be empty.");
            }

            try
            {
                var tickets = requests.Select(request => new TicketModel(
                    request.TicketId,
                    request.AccountId,
                    request.Content,
                    request.Type,
                    request.IsResolved));
                var count = await _ticketApplication.UpdateRangeAsync(
                    tickets, cancellationToken);
                return Ok(new { affectedRows = count });
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Ticket batch update request failed.");
            }
        }

        #endregion

        private IActionResult HandleError(Exception exception, string message)
        {
            if (exception is OperationCanceledException)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Ticket request was canceled.");
                return BadRequest("Request was canceled.");
            }

            _logger.LogLayerError(_logPool, Module, Layer, exception, message);
            return BadRequest(exception.Message);
        }
    }
}
