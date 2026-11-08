using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Models.Profile;
using Identity.Presentation.Record.Profile;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging;
using Shared.ModelHelper;
using Shared.Presentation.Controller;
using System.Text.RegularExpressions;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("api/profile/profile")]
    public partial class ProfileController(
        IUserProfileApplication userProfileApplication,
        IMappingHelper apiHelper,
        ILogger<ProfileController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, "Identity", "Presentation/Controller")
    {
        private readonly IMappingHelper _apiHelper = apiHelper;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ProfileController> _logger = logger;

        private readonly IUserProfileApplication _userProfileApplication = userProfileApplication;

        #region PRIVATE

        private static bool IsValidIdentityCode(string? identityCode) =>
            identityCode is { Length: 12 } && identityCode.All(char.IsDigit);

        private static bool IsValidGetProfileRequest(RecordGetProfileRequest request) =>
            request.AccountId.HasValue
                ? request.AccountId.Value != Guid.Empty
                : IsValidIdentityCode(request.IdentityCode);

        private static bool IsValidCreateProfileRequest(RecordCreateBaseProfileRequest request) =>
            request.AccountId != Guid.Empty && IsValidIdentityCode(request.IdentityCode);

        private static bool TryCreateUpdatedProfile(
            RecordUpdateProfileRequest request,
            out UserProfileModel profile,
            out string error)
        {
            profile = null!;
            error = string.Empty;
            if (!IsValidIdentityCode(request.IdentityCode) || request.AccountId == Guid.Empty)
            {
                error = "A valid identity code and account id are required.";
                return false;
            }

            if (request.FirstName?.Length > 30 || request.LastName?.Length > 30 ||
                request.Address?.Length > 255 || request.PhoneNumber?.Length > 50 ||
                !Enum.IsDefined(request.UserGender))
            {
                error = "One or more profile fields are invalid.";
                return false;
            }

            if (request.DateOfBirth.HasValue && (request.DateOfBirth.Value > DateTimeOffset.UtcNow || request.DateOfBirth.Value < DateTimeOffset.UtcNow.AddYears(-100) 
                    || request.DateOfBirth.Value > DateTimeOffset.UtcNow.AddYears(-18)))
            {
                error = "Date of birth cannot be in the future. It must be within the last 100 years and at least 18 years ago.";
                return false;
            }
            
            if (!TryNormalizePhoneNumber(request.PhoneNumber, out var normalizedPhone))
            {
                error = "Phone number format is invalid.";
                return false;
            }

            try
            {
                ImageDataUriGuard.OptionalAvatar(request.AvatarUrl, nameof(request.AvatarUrl));
            }
            catch (ArgumentException)
            {
                error = "Avatar data is invalid.";
                return false;
            }

            profile = new UserProfileModel(request.IdentityCode, request.AccountId, request.FirstName,
                request.LastName, request.DateOfBirth, request.UserGender, normalizedPhone,
                request.Address, request.AvatarUrl);
            return true;
        }

        private static bool TryNormalizePhoneNumber(string? input, out string? normalizedPhone)
        {
            normalizedPhone = input;
            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            var cleaned = PhoneRegex().Replace(input, string.Empty);
            if (cleaned.StartsWith("+84", StringComparison.Ordinal))
            {
                cleaned = string.Concat("0", cleaned.AsSpan(3));
            }
            else if (cleaned.StartsWith("84", StringComparison.Ordinal) && cleaned.Length == 11)
            {
                cleaned = string.Concat("0", cleaned.AsSpan(2));
            }

            if (!MyRegex().IsMatch(cleaned))
            {
                return false;
            }

            normalizedPhone = cleaned;
            return true;
        }

        #endregion

        #region GET

        [RequireHttps]
        [HttpGet]
        public async Task<ActionResult<RecordProfileResponse>> GetProfileAsync([FromQuery] RecordGetProfileRequest requestDto, CancellationToken cancellationToken)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Profile lookup request received.");
            if (!IsValidGetProfileRequest(requestDto))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Profile lookup request rejected because its identifier is invalid.");
                return BadRequest("A valid account id or 12-digit identity code is required.");
            }
            try
            {
                UserProfileModel? result;
                if (requestDto.AccountId != null)
                {
                    result = await _userProfileApplication.GetProfileInfoAsync(requestDto.AccountId!.Value, cancellationToken);
                }
                else
                {
                    result = await _userProfileApplication.GetProfileInfoAsync(requestDto.IdentityCode!, cancellationToken);
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
            if (!IsValidCreateProfileRequest(requestDto))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Profile creation request rejected by input validation.");
                return BadRequest("A valid account id and 12-digit identity code are required.");
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
        [HttpPut("update")]
        public async Task<ActionResult<RecordProfileResponse>> UpdateUserProfileAsync([FromBody] RecordUpdateProfileRequest requestDto, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update request received.");
            if (!TryCreateUpdatedProfile(requestDto, out var newProfileModel, out var validationError))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Profile update request rejected by input validation.");
                return BadRequest(validationError);
            }

            try
            {
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

        [GeneratedRegex(@"^(03|05|07|08|09)\d{8}$")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"[^\d+]")]
        private static partial Regex PhoneRegex();

        #endregion
    }
}
