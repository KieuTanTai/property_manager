using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Models.Profile;
using Identity.Presentation.Record.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Logging;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("api/profile/profile")]
    public class ProfileController(
        IUserProfileApplication userProfileApplication,
        IApiHelper apiHelper,
        ILogger<ProfileController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, Module, Layer)
    {
        private readonly IApiHelper _apiHelper = apiHelper;

        private readonly IUserProfileApplication _userProfileApplication = userProfileApplication;
        private readonly ILogger<ProfileController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        private const string Module = "identity";
        private const string Layer = "controller";

        #region GET

        [RequireHttps]
        [HttpGet]
        public async Task<ActionResult<RecordProfileResponse>> GetProfileAsync([FromQuery] RecordGetProfileRequest requestDto, CancellationToken cancellationToken)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Profile lookup request received.");
            if (requestDto.IdentityCode == null && requestDto.AccountId == null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Profile lookup request rejected because no identifier was provided.");
                return BadRequest("must have at least one id");
            }
            try
            {
                UserProfileModel? result;
                if (requestDto.AccountId != null)
                {
                    result = await _userProfileApplication.GetProfileInfoAsync(requestDto.AccountId, cancellationToken);
                }
                else
                {
                    result = await _userProfileApplication.GetProfileInfoAsync(requestDto.IdentityCode, cancellationToken);
                }
                var response = _apiHelper.MappingProfileResult(result);
                return Ok(response);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Profile lookup request was canceled.");
                return BadRequest($"Operation canceled \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Profile lookup request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        #endregion

        #region POST

        [RequireHttps]
        [HttpPost("create")]
        public async Task<ActionResult<RecordProfileResponse>> CreateProfileAsync([FromBody] RecordCreateBaseProfileRequest requestDto,
            CancellationToken cancellationToken)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile creation request received.");
            if (string.IsNullOrWhiteSpace(requestDto.IdentityCode) || requestDto.IdentityCode.Length != 12)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Profile creation request rejected because the identity code is invalid.");
                return BadRequest("identity code is required and must be 12 digits");
            }

            try
            {
                var result = await _userProfileApplication.CreateBaseProfileInfoAsync(requestDto.IdentityCode, requestDto.AccountId, cancellationToken);
                var response = _apiHelper.MappingProfileResult(result);
                return Ok(response);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Profile creation request was canceled.");
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Profile creation request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPost("update")]
        public async Task<ActionResult<RecordProfileResponse>> UpdateUserProfileAsync([FromBody] RecordUpdateProfileRequest requestDto, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update request received.");
            try
            {
                var newProfileModel = new UserProfileModel(requestDto.IdentityCode, requestDto.AccountId, requestDto.FirstName, requestDto.LastName,
                    requestDto.DateOfBirth, requestDto.UserGender, requestDto.PhoneNumber, requestDto.Address, requestDto.AvatarUrl);
                var result = await _userProfileApplication.UpdateProfileInfoAsync(newProfileModel, cancellationToken);
                var response = _apiHelper.MappingProfileResult(result);
                return Ok(response);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Profile update request was canceled.");
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Profile update request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        #endregion
    }
}