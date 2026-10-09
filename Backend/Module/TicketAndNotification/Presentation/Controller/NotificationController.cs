using Microsoft.AspNetCore.Mvc;
using Shared.Enum;
using Shared.Logging;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Models.Notification;
using TicketAndNotification.Presentation.Record.Notification;

namespace TicketAndNotification.Presentation.Controller
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController(
        INotificationApplication notificationApplication,
        ILogger<NotificationController> logger,
        ILogPool logPool) : ControllerBase
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Presentation/Controller";

        private readonly INotificationApplication _notificationApplication = notificationApplication;
        private readonly ILogger<NotificationController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _notificationApplication.GetAllAsync(cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification list request failed.");
            }
        }

        [HttpGet("{notificationId:guid}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _notificationApplication.GetByIdAsync(
                    notificationId, cancellationToken);
                return notification is null
                    ? NotFound("Notification was not found.")
                    : Ok(notification);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification lookup request failed.");
            }
        }

        [HttpGet("by-ids")]
        public async Task<IActionResult> GetByIdsAsync(
            [FromQuery] List<Guid> notificationIds,
            CancellationToken cancellationToken = default)
        {
            if (notificationIds.Count == 0 || notificationIds.Contains(Guid.Empty))
            {
                return BadRequest("At least one valid notification id is required.");
            }

            try
            {
                return Ok(await _notificationApplication.GetByIdsAsync(
                    notificationIds, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification lookup by ids failed.");
            }
        }

        [HttpGet("by-sender/{senderAccountId:guid}")]
        public async Task<IActionResult> GetBySenderAccountIdAsync(
            [FromRoute] Guid senderAccountId,
            CancellationToken cancellationToken = default)
        {
            if (senderAccountId == Guid.Empty)
            {
                return BadRequest("A valid sender account id is required.");
            }

            try
            {
                return Ok(await _notificationApplication.GetBySenderAccountIdAsync(
                    senderAccountId, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification sender lookup failed.");
            }
        }

        [HttpGet("by-recipient/{recipientAccountId:guid}")]
        public async Task<IActionResult> GetByRecipientAccountIdAsync(
            [FromRoute] Guid recipientAccountId,
            CancellationToken cancellationToken = default)
        {
            if (recipientAccountId == Guid.Empty)
            {
                return BadRequest("A valid recipient account id is required.");
            }

            try
            {
                return Ok(await _notificationApplication.GetByRecipientAccountIdAsync(
                    recipientAccountId, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification recipient lookup failed.");
            }
        }

        [HttpGet("by-type/{type}")]
        public async Task<IActionResult> GetByTypeAsync(
            [FromRoute] ENotificationType type,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(type))
            {
                return BadRequest("Notification type is invalid.");
            }

            try
            {
                return Ok(await _notificationApplication.GetByTypeAsync(
                    type, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification type lookup failed.");
            }
        }

        [HttpGet("by-read-status/{isRead:bool}")]
        public async Task<IActionResult> GetByIsReadAsync(
            [FromRoute] bool isRead,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _notificationApplication.GetByIsReadAsync(
                    isRead, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification read status lookup failed.");
            }
        }

        #endregion

        #region POST

        [HttpPost]
        public async Task<IActionResult> AddAsync(
            [FromBody] CreateNotificationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new NotificationModel(
                    request.SenderAccountId, request.Content, request.Type);
                var result = await _notificationApplication.AddAsync(
                    notification, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync),
                    new { notificationId = result.NotificationId }, result);
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification creation request failed.");
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> AddRangeAsync(
            [FromBody] List<CreateNotificationRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return BadRequest("Notification collection cannot be empty.");
            }

            try
            {
                var notifications = requests.Select(request =>
                    new NotificationModel(
                        request.SenderAccountId, request.Content, request.Type));
                var count = await _notificationApplication.AddRangeAsync(
                    notifications, cancellationToken);
                return Ok(new { affectedRows = count });
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification batch creation failed.");
            }
        }

        #endregion

        #region PUT

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(
            [FromBody] UpdateNotificationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new NotificationModel(
                    request.NotificationId,
                    request.SenderAccountId,
                    request.Type,
                    request.Content,
                    request.IsRead);
                return Ok(await _notificationApplication.UpdateAsync(
                    notification, cancellationToken));
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification update request failed.");
            }
        }

        [HttpPut("batch")]
        public async Task<IActionResult> UpdateRangeAsync(
            [FromBody] List<UpdateNotificationRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return BadRequest("Notification collection cannot be empty.");
            }

            try
            {
                var notifications = requests.Select(request =>
                    new NotificationModel(
                        request.NotificationId,
                        request.SenderAccountId,
                        request.Type,
                        request.Content,
                        request.IsRead));
                var count = await _notificationApplication.UpdateRangeAsync(
                    notifications, cancellationToken);
                return Ok(new { affectedRows = count });
            }
            catch (Exception exception)
            {
                return HandleError(exception, "Notification batch update failed.");
            }
        }

        #endregion

        private IActionResult HandleError(Exception exception, string message)
        {
            if (exception is OperationCanceledException)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer,
                    "Notification request was canceled.");
                return BadRequest("Request was canceled.");
            }

            _logger.LogLayerError(_logPool, Module, Layer, exception, message);
            return BadRequest(exception.Message);
        }
    }
}
