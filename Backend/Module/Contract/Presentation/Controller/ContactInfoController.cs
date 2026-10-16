using Contract.Interfaces.IApplication;
using Contract.Models.Contact;
using Contract.Presentation.Record.Contact;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging;
using Shared.Presentation.Controller;

namespace Contract.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactInfoController(
        IContactInfoApplication contactInfoApplication,
        ILogger<ContactInfoController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, "Contract", "Presentation/Controller")
    {
        private readonly IContactInfoApplication _contactInfoApplication = contactInfoApplication;
        private readonly ILogger<ContactInfoController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        [RequireHttps]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _contactInfoApplication.GetAllAsync(cancellationToken);
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, ex.Message);
                return BadRequest("The request was canceled." + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contact info retrieval request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #region POST

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
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, ex.Message);
                return BadRequest("The request was canceled." + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contact info creation request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

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
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, ex.Message);
                return BadRequest("The request was canceled." + ex.Message);
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
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, ex.Message);
                return BadRequest("The request was canceled." + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Contact info deletion request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion
    }
}
