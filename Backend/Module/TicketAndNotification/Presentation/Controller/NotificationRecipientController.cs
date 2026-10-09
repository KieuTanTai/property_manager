using Microsoft.AspNetCore.Mvc;
using Shared.Logging;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Models.Notification;
using TicketAndNotification.Presentation.Record.NotificationRecipient;

namespace TicketAndNotification.Presentation.Controller
{
    [ApiController]
    [Route("api/notification-recipients")]
    public class NotificationRecipientController(
        INotificationRecipientApplication recipientApplication,
        ILogger<NotificationRecipientController> logger,
        ILogPool logPool) : ControllerBase
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Presentation/Controller";

        private readonly INotificationRecipientApplication _recipientApplication = recipientApplication;
        private readonly ILogger<NotificationRecipientController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _recipientApplication.GetAllAsync(cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification recipient list request failed.");
            }
        }

        [HttpGet("by-notification/{notificationId:guid}")]
        public async Task<IActionResult> GetByNotificationIdAsync(
            [FromRoute] Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            if (notificationId == Guid.Empty)
            {
                return BadRequest("A valid notification id is required.");
            }

            try
            {
                return Ok(await _recipientApplication.GetByNotificationIdAsync(
                    notificationId, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification recipient lookup failed.");
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
                return Ok(await _recipientApplication.GetByAccountIdAsync(
                    accountId, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification recipient account lookup failed.");
            }
        }

        [HttpGet("{notificationId:guid}/{accountId:guid}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] Guid notificationId,
            [FromRoute] Guid accountId,
            CancellationToken cancellationToken = default)
        {
            if (notificationId == Guid.Empty || accountId == Guid.Empty)
            {
                return BadRequest("Valid notification and account ids are required.");
            }

            try
            {
                var recipient = await _recipientApplication.GetByIdAsync(
                    notificationId, accountId, cancellationToken);
                return recipient is null
                    ? NotFound("Notification recipient was not found.")
                    : Ok(recipient);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification recipient lookup failed.");
            }
        }

        #endregion

        #region POST

        [HttpPost]
        public async Task<IActionResult> AddAsync(
            [FromBody] CreateNotificationRecipientRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var recipient = new NotificationRecipientModel(
                    request.NotificationId, request.AccountId);
                var result = await _recipientApplication.AddAsync(
                    recipient, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync),
                    new
                    {
                        notificationId = result.NotificationId,
                        accountId = result.AccountId
                    },
                    result);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification recipient creation failed.");
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> AddRangeAsync(
            [FromBody] List<CreateNotificationRecipientRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return BadRequest("Notification recipient collection cannot be empty.");
            }

            try
            {
                var recipients = requests.Select(request =>
                    new NotificationRecipientModel(
                        request.NotificationId, request.AccountId));
                var count = await _recipientApplication.AddRangeAsync(
                    recipients, cancellationToken);
                return Ok(new { affectedRows = count });
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification recipient batch creation failed.");
            }
        }

        #endregion

        private IActionResult HandleError(Exception exception, string message)
        {
            if (exception is OperationCanceledException)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Notification recipient request was canceled.");
                return BadRequest("Request was canceled.");
            }

            _logger.LogLayerError(_logPool, Module, Layer, exception, message);
            return BadRequest(exception.Message);
        }
    }
}
