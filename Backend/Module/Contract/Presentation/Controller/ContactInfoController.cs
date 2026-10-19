using Contract.Interfaces.IApplication;
using Contract.Interfaces.Services;
using Contract.Models.Contact;
using Contract.Presentation.Record.Contact;
using Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Logging;
using Shared.Presentation.Controller;

namespace Contract.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactInfoController(
        IContactInfoApplication contactInfoApplication,
        IEmailSender emailSender,
        IIdentityHelper<SendConsultationRequest> identityHelper,
        ILogger<ContactInfoController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, "Contract", "Presentation/Controller")
    {
        private readonly IContactInfoApplication _contactInfoApplication = contactInfoApplication;

        private readonly IEmailSender _emailSender = emailSender;

        private readonly IIdentityHelper<SendConsultationRequest> _identityHelper = identityHelper;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ContactInfoController> _logger = logger;

        [RequireHttps]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _contactInfoApplication.GetAllAsync(cancellationToken);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Contact info retrieval request was canceled.");
                return BadRequest("The request was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contact info retrieval request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #region PUT

        [RequireHttps]
        [HttpPut("{contactInfoId:guid}")]
        public async Task<IActionResult> UpdateAsync(
            Guid contactInfoId,
            [FromBody] RecordContactInfoRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var contactInfo = new ContactInfoModel(
                    contactInfoId, request.ContactInfoEmail, request.ContactInfoPhone, request.ContactInfoAddress,
                    request.ContactSocialLink);
                var affectedRows = await _contactInfoApplication.UpdateAsync(contactInfo, cancellationToken);
                return affectedRows == 0 ? BadRequest("Could not update contact info.") : Ok(contactInfo);
            }
            catch (OperationCanceledException)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Contact info update request was canceled.");
                return BadRequest("The request was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contact info update request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region DELETE

        [RequireHttps]
        [HttpDelete("{contactInfoId:guid}")]
        public async Task<IActionResult> DeleteAsync(
            Guid contactInfoId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var affectedRows = await _contactInfoApplication.DeleteAsync(contactInfoId, cancellationToken);
                return affectedRows == 0 ? BadRequest("Could not delete contact info.") : NoContent();
            }
            catch (OperationCanceledException)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Contact info deletion request was canceled.");
                return BadRequest("The request was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contact info deletion request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region POST

        [RequireHttps]
        [HttpPost("consultation")]
        [EnableRateLimiting("consultation-email")]
        public async Task<IActionResult> SendConsultationAsync(
            [FromBody] SendConsultationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_identityHelper.IsEmailValid(request.SenderEmail))
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer, $"Invalid sender email format");
                    return BadRequest("Sender email is invalid.");
                }

                if (!_identityHelper.IsPhoneNumberValid(request.Phone))
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer, $"Invalid phone number format");
                    return BadRequest("Phone number is invalid.");
                }
                
                var premise = string.IsNullOrWhiteSpace(request.PremiseName)
                    ? "Chưa chọn"
                    : request.PremiseName.Trim();
                var subjectName = request.Name.Replace('\r', ' ').Replace('\n', ' ').Trim();
                var senderEmail = request.SenderEmail.Trim();
                var subject = $"Yêu cầu tư vấn mặt bằng - {subjectName}";
                var body = $"Yêu cầu tư vấn mới từ website ManagerSpace\n\n"
                           + $"Họ tên: {request.Name.Trim()}\n"
                           + $"Số điện thoại: {request.Phone.Trim()}\n"
                           + $"Email: {senderEmail}\n"
                           + $"Mặt bằng quan tâm: {premise}\n"
                           + $"Nội dung: {(string.IsNullOrWhiteSpace(request.Message) ? "Vui lòng liên hệ tư vấn." : request.Message.Trim())}";

                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Sending a consultation request email.");
                var recipientEmail = await _emailSender.SendAsync(
                    senderEmail,
                    subject,
                    body,
                    cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Consultation email request was sent successfully: {recipientEmail}");
                return Ok(new { recipientEmail });
            }
            catch (OperationCanceledException)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Consultation email request was canceled.");
                return BadRequest("The request was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex,
                    "Failed to send the consultation email.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    "Không gửi được email. Vui lòng thử lại sau hoặc liên hệ ban quản lý.");
            }
        }

        [RequireHttps]
        [HttpPost]
        public async Task<IActionResult> AddAsync(
            [FromBody] RecordContactInfoRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var contactInfo = new ContactInfoModel(
                    request.ContactInfoEmail, request.ContactInfoPhone, request.ContactInfoAddress, request.ContactSocialLink);
                return Ok(await _contactInfoApplication.AddAsync(contactInfo, cancellationToken));
            }
            catch (OperationCanceledException)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Contact info creation request was canceled.");
                return BadRequest("The request was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contact info creation request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion
    }
}
