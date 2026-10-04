using System.Security.Claims;
using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Models.Account;
using Identity.Presentation.Record.Account;
using Microsoft.AspNetCore.Mvc;
using Shared.Persistence.Record;
using Shared.Persistence.Record.Auth;
using Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(
        IAccountApplication accountApplication,
        IUserProfileApplication userProfileApplication,
        IAccountHelper helper,
        IApiHelper apiHelper,
        ILogger<AccountController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, "Identity", "Presentation/Controller")
    {
        private readonly IAccountApplication _accountApplication = accountApplication;

        private readonly IApiHelper _apiHelper = apiHelper;

        private readonly IAccountHelper _helper = helper;

        private readonly IUserProfileApplication _userProfileApplication = userProfileApplication;
        private readonly ILogger<AccountController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region PRIVATE

        private static List<RecordClaimResponse> CreateClaims(RecordAuthResponse account)
        {
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString());
            var emailClaim = new Claim(ClaimTypes.Email, account.Email);

            var roleClaims = account.RoleCodes.Select(roleCode => new Claim(ClaimTypes.Role, roleCode));
            var additionalPermissionClaims = account.PermissionCodes.Select(permissionCode => new Claim("AdditionalPermission", permissionCode));
            var claims = new List<Claim> { nameIdentifierClaim, emailClaim };
            claims.AddRange(roleClaims);
            claims.AddRange(additionalPermissionClaims);
            return [.. claims.Select(claim => new RecordClaimResponse(claim.Type, claim.Value))];
        }
        
        #endregion
        
        #region GET

        [RequireHttps]
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalyticsAccountAsync([FromQuery] Guid? cursor, [FromQuery] int pageSize = 5, [FromQuery] bool isGetProfile = true, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account analytics request received.");

            if (pageSize <= 0)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account analytics request rejected because page size is invalid.");
                return BadRequest("Page size must be greater than 0.");
            }
            
            try
            {
                var result = await _accountApplication.GetApplyPagingAsync(cursor, pageSize, isGetProfile, cancellationToken);

                if (result.Items.Count == 0)
                    return NotFound("No accounts found.");
                return Ok(result);
                
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Account analytics request was canceled.");
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account analytics request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("status")]
        public async Task<IActionResult> GetAccountsByStatusAsync(
            [FromQuery] bool isActive,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account status list request received.");
            if (pageSize <= 0)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account status request rejected because page size is invalid.");
                return BadRequest("Page size must be greater than 0.");
            }

            try
            {
                var result = await _accountApplication.GetApplyPagingByStatusAsync(
                    cursor, pageSize, isActive, cancellationToken);
                return result.Items.Count == 0 ? NotFound("No accounts found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Account status request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account status list request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("email")]
        public async Task<IActionResult> GetAccountByEmailAsync(
            [FromQuery] string email,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account lookup request received.");
            if (string.IsNullOrWhiteSpace(email) || !_helper.IsEmailValid(email))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account lookup request rejected because email format is invalid.");
                return BadRequest("A valid email is required.");
            }

            try
            {
                var result = await _accountApplication.GetAccountByEmailAsync(email, cancellationToken);
                return result is null ? NotFound("Account not found.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Account lookup request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account lookup request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion
        
        #region POST

        [RequireHttps]
        [HttpPost("register")]
        public async Task<ActionResult<RecordAuthResponse>> RegisterAsync([FromBody] RecordAuthRequest requestDto, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account registration request received.");
            var (isValid, errorMessage) = _helper.ValidateEmailAndPassword(requestDto.Email, requestDto.Password);
            if (!isValid)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account registration request rejected because validation failed.");
                return BadRequest(errorMessage);
            }

            try
            {
                var result = await _accountApplication.RegisterAsync(requestDto.Email, requestDto.Password, cancellationToken);
                var response = _apiHelper.MappingAuthResult(result);
                return Ok(response);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Account registration request was canceled.");
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account registration request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] RecordAuthRequest requestDto, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account login request received.");
            var (isValid, errorMessage) = _helper.ValidateEmailAndPassword(requestDto.Email, requestDto.Password);
            if (!isValid)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account login request rejected because validation failed.");
                return BadRequest(errorMessage);
            }
            try
            {
                var result = await _accountApplication.LoginAsync(requestDto.Email, requestDto.Password, cancellationToken);
                var account = _apiHelper.MappingAuthResult(result);
                var profile = result.UserProfile is null
                    ? null
                    : _apiHelper.MappingProfileResult(result.UserProfile);
                var claims = CreateClaims(account);
                return Ok(new RecordLoginResponse(account, profile, claims));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Account login request was canceled.");
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account login request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        /// ! NOTE: DON'T USE THIS ENDPOINT FOR LOGOUT, IT WILL CALL A NOT IMPLEMENTED METHOD ON APPLICATION LAYER
        [RequireHttps]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync(
            [FromQuery] string email,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account logout request received.");
            if (string.IsNullOrWhiteSpace(email) || !_helper.IsEmailValid(email))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account logout request rejected because the account identifier is invalid.");
                return BadRequest("A valid email is required.");
            }

            try
            {
                var result = await _accountApplication.LogoutAsync(email, cancellationToken);
                return result ? Ok(result) : BadRequest("Could not logout account.");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Account logout request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account logout request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPost("password/change")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] RecordUpdateAccountPasswordRequest requestDto, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account password change request received.");
            var (isValid, errorMessage) = _helper.ValidateEmailAndPassword(requestDto.Email, requestDto.OldPassword);
            if (!isValid)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change request rejected because validation failed.");
                return BadRequest(errorMessage);
            }
            if (string.IsNullOrWhiteSpace(requestDto.NewPassword))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change request rejected because the new password is missing.");
                return BadRequest("New password is required.");
            }
            if (string.CompareOrdinal(requestDto.OldPassword, requestDto.NewPassword) == 0)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change request rejected because passwords are equal.");
                return BadRequest("New password must be different from old password.");
            }

            try
            {
                var result = await _accountApplication.ChangePasswordAsync(requestDto.Email, requestDto.OldPassword, requestDto.NewPassword, cancellationToken);
                if (result == 0)
                {
                    return BadRequest("Could not change password.");
                }
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Password change request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPost("active")]
        public async Task<IActionResult> ActiveAccountByAdminAsync([FromBody] string email, CancellationToken cancellationToken = default)
        {
            if (!_helper.IsEmailValid(email))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account activation request rejected because the account identifier is invalid.");
                return BadRequest("A valid email is required.");
            }
            try
            {
                var result = await _accountApplication.ActiveAccountByAdminAsync(email, cancellationToken);
                if (result == 0)
                {
                    return BadRequest("Could not active account.");
                }
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account activation request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }
        
        #endregion

        #region DELETE

        [RequireHttps]
        [HttpDelete("delete")]
        public async Task<IActionResult> InactiveAccountAsync([FromBody] RecordInactiveAccountRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (isValid, errorMessage) = _helper.ValidateEmailAndPassword(requestDto.Email, requestDto.Password);
            if (!isValid)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account deactivation request rejected because validation failed.");
                return BadRequest(errorMessage);
            }
            if (string.CompareOrdinal(requestDto.Password, requestDto.ConfirmPassword) != 0)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account deactivation request rejected because confirmation failed.");
                return BadRequest("Password and confirm password do not match.");
            }

            try
            {
                var result = await _accountApplication.InactiveAccountAsync(requestDto.Email, requestDto.Password, cancellationToken);
                if (result == 0)
                {
                    return BadRequest("Could not inactive account.");
                }
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account deactivation request failed.");
                return BadRequest(ex.Message);
            }
        }

        [RequireHttps]
        [HttpDelete("delete/{accountId}")]
        public async Task<IActionResult> InactiveAccountByAdminAsync([FromRoute] string accountId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Admin account deactivation request rejected because the account identifier is missing.");
                return BadRequest("Account id is required.");
            }

            try
            {
                var result = await _accountApplication.InactiveAccountByAdminAsync(new Guid(accountId), cancellationToken);
                if (result == 0)
                {
                    return BadRequest("Could not inactive account.");
                }
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Admin account deactivation request failed.");
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}