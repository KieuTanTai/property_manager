using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Models.Account;
using Identity.Presentation.Record.Account;
using Microsoft.AspNetCore.Mvc;
using Shared.Enum;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record.Auth;
using Shared.Presentation.Controller;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(
        IAccountApplication accountApplication,
        IIdentityHelper<AccountModel> helper,
        IMappingHelper apiHelper,
        ILogger<AccountController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, "Identity", "Presentation/Controller")
    {
        private static readonly string SessionInstanceId = Guid.NewGuid().ToString("N");

        private readonly IAccountApplication _accountApplication = accountApplication;

        private readonly IMappingHelper _apiHelper = apiHelper;

        private readonly IIdentityHelper<AccountModel> _helper = helper;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<AccountController> _logger = logger;

        #region PRIVATE

        private bool IsValidCredentials(string email, string password) =>
            _helper.IsEmailValid(email) && _helper.IsPasswordValid(password);

        private bool IsValidPasswordChange(RecordUpdateAccountPasswordRequest request) =>
            IsValidCredentials(request.Email, request.OldPassword) &&
            _helper.IsPasswordValid(request.NewPassword) &&
            !string.Equals(request.OldPassword, request.NewPassword, StringComparison.Ordinal);

        private bool IsValidAccountModel(AccountModel account, bool requireId)
        {
            return (!requireId || account.AccountId != Guid.Empty) &&
                   _helper.IsEmailValid(account.AccountEmail) &&
                   !string.IsNullOrWhiteSpace(account.AccountPassword) && account.AccountPassword.Length <= 255;
        }

        private bool AreValidAccountsForUpdate(IReadOnlyCollection<AccountModel>? accounts) =>
            accounts is { Count: > 0 } && accounts.All(account => IsValidAccountModel(account, true));

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
                {
                    return NotFound("No accounts found.");
                }
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
        [HttpGet("role")]
        public async Task<IActionResult> GetAllAccountsByRoleCodeAsync(
            [FromQuery] ESystemRoleCode roleCode,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Get all accounts by role code request received.");
            if (!Enum.IsDefined(roleCode))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account role lookup request rejected because role code is invalid.");
                return BadRequest("Role code is invalid.");
            }

            try
            {
                var result = await _accountApplication.GetAllAccountsByRoleCodeAsync(roleCode, cancellationToken);
                if (result.Count == 0)
                {
                    return NotFound("No accounts found.");
                }

                var accounts = result.Select(account => new RecordAccountProfileOption(
                    account.AccountId,
                    account.AccountEmail,
                    account.UserProfile is null ? null : _apiHelper.MappingProfileResult(account.UserProfile)));
                return Ok(accounts);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Get all accounts by role code request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Get all accounts by role code request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
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
            if (!_helper.IsEmailValid(email))
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
            if (!IsValidCredentials(requestDto.Email, requestDto.Password))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account registration request rejected because email or password is invalid.");
                return BadRequest("A valid email and password are required.");
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
            if (!IsValidCredentials(requestDto.Email, requestDto.Password))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account login request rejected because email or password is invalid.");
                return BadRequest("A valid email and password are required.");
            }

            try
            {
                var response = await _accountApplication.LoginAsync(requestDto.Email, requestDto.Password, cancellationToken);
                return Ok(response);
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

        [HttpGet("session-instance")]
        public IActionResult GetSessionInstance()
        {
            Response.Headers.CacheControl = "no-store";
            return Ok(new { sessionInstanceId = SessionInstanceId });
        }

        /// ! NOTE: DON'T USE THIS ENDPOINT FOR LOGOUT, IT WILL CALL A NOT IMPLEMENTED METHOD ON APPLICATION LAYER
        [RequireHttps]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync(
            [FromQuery] string email,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account logout request received.");
            if (!_helper.IsEmailValid(email))
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
        [HttpPut("password/change")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] RecordUpdateAccountPasswordRequest requestDto, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account password change request received.");
            
            if (!IsValidPasswordChange(requestDto))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change request rejected because credentials are invalid.");
                return BadRequest("Email and passwords must be valid, and the new password must differ from the old password.");
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
        [HttpPut("active")]
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

        [RequireHttps]
        [HttpPut("update-range")]
        public async Task<IActionResult> UpdateAccountsRangeAsync([FromBody] List<AccountModel> accounts, CancellationToken cancellationToken = default)
        {
            if (!AreValidAccountsForUpdate(accounts))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account update range request rejected by input validation.");
                return BadRequest("Account updates must contain valid account identifiers, emails, and passwords.");
            }

            try
            {
                var result = await _accountApplication.UpdateRange(accounts, cancellationToken);
                if (result == 0)
                {
                    return BadRequest("Could not update accounts.");
                }
                return Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Account update range request failed.");
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        #endregion

        #region DELETE

        [RequireHttps]
        [HttpDelete("delete")]
        public async Task<IActionResult> InactiveAccountAsync([FromBody] RecordInactiveAccountRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (!IsValidCredentials(requestDto.Email, requestDto.Password) ||
                !string.Equals(requestDto.Password, requestDto.ConfirmPassword, StringComparison.Ordinal))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account deactivation request rejected by input validation.");
                return BadRequest("A valid email and matching passwords are required.");
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
            if (!Guid.TryParse(accountId, out var parsedAccountId) || parsedAccountId == Guid.Empty)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Admin account deactivation request rejected because the account identifier is invalid.");
                return BadRequest("A valid account id is required.");
            }

            try
            {
                var result = await _accountApplication.InactiveAccountByAdminAsync(parsedAccountId, cancellationToken);
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
