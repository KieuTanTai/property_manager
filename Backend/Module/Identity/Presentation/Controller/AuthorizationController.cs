using Identity.Interfaces.IApplication;
using Identity.Models.Permission;
using Identity.Models.Role;
using Identity.Presentation.Record.Authorization;
using Identity.Utils.Enum;
using Microsoft.AspNetCore.Mvc;
using Shared.ModelHelper;
using Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController(
        IAuthorizationApplication authorizationApplication,
        ILogger<AuthorizationController> logger,
        ILogPool logPool) : CustomControllerBase(logPool, Module, Layer)
    {
        private readonly IAuthorizationApplication _authorizationApplication = authorizationApplication;
        private readonly ILogger<AuthorizationController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        private const string Module = "identity";
        private const string Layer = "controller";

        #region GET

        [RequireHttps]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Role list request received.");
            try
            {
                var roles = await _authorizationApplication.GetAllRolesWithPermissionsAsync(cancellationToken);
                if (!roles.Any())
                    return NotFound("No roles found.");
                return Ok(roles);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Role list request was canceled.");
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("roles/{roleCode}")]
        public async Task<IActionResult> GetRoleAsync(
            [FromRoute] ESystemRoleCode roleCode,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Role lookup request received.");
            if (!Enum.IsDefined(roleCode))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Role lookup request rejected because role code is invalid.");
                return BadRequest("Role code is invalid.");
            }

            try
            {
                var role = await _authorizationApplication.GetRoleWithPermissionsAsync(roleCode, cancellationToken);
                return Ok(role);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Role lookup request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissionsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Permission list request received.");
            try
            {
                var permissions = await _authorizationApplication.GetAllPermissionsAsync(cancellationToken);
                if (!permissions.Any())
                    return NotFound("No permissions found.");
                return Ok(permissions);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Permission list request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpGet("permissions/{permissionCode}")]
        public async Task<IActionResult> GetPermissionAsync(
            [FromRoute] ESystemPermissionCode permissionCode,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Permission lookup request received.");
            if (!Enum.IsDefined(permissionCode))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Permission lookup request rejected because permission code is invalid.");
                return BadRequest("Permission code is invalid.");
            }

            try
            {
                var permission = await _authorizationApplication.GetPermissionByCodeAsync(permissionCode, cancellationToken);
                return Ok(permission);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Permission lookup request failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region POST

        [RequireHttps]
        [HttpPost("roles")]
        public async Task<IActionResult> AddRoleAsync(
            [FromBody] RoleChangeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryCreateRole(request, out var role, out var error))
            {
                return BadRequest(error);
            }

            try
            {
                var result = await _authorizationApplication.AddRoleAsync(role, request.PermissionIds, cancellationToken);
                return result == 0 ? BadRequest("Could not add role.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization operation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPost("accounts/{accountId:guid}/roles")]
        public async Task<IActionResult> AddAccountRolesAsync(
            [FromRoute] Guid accountId,
            [FromBody] IdListRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelFieldGuard.ValidateIds(accountId, request.Ids))
            {
                return BadRequest("Account id and at least one valid role id are required.");
            }

            try
            {
                var result = await _authorizationApplication.AddAccountRolesAsync(
                    accountId, request.Ids, cancellationToken);
                return result == 0 ? BadRequest("Could not add account roles.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization operation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPost("permissions")]
        public async Task<IActionResult> AddPermissionAsync(
            [FromBody] PermissionChangeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryCreatePermission(request, out var permission, out var error))
            {
                return BadRequest(error);
            }

            try
            {
                var result = await _authorizationApplication.AddPermissionAsync(permission, cancellationToken);
                return result == 0 ? BadRequest("Could not add permission.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization operation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPost("accounts/{accountId:guid}/permissions")]
        public async Task<IActionResult> AddAccountPermissionsAsync(
            [FromRoute] Guid accountId,
            [FromBody] IdListRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelFieldGuard.ValidateIds(accountId, request.Ids))
            {
                return BadRequest("Account id and at least one valid permission id are required.");
            }

            try
            {
                var result = await _authorizationApplication.AddAccountAdditionalPermissionsAsync(
                    accountId, request.Ids, cancellationToken);
                return result == 0 ? BadRequest("Could not add account permissions.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization operation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region PUT

        [RequireHttps]
        [HttpPut("roles/{roleId:guid}")]
        public async Task<IActionResult> UpdateRoleAsync(
            [FromRoute] Guid roleId,
            [FromBody] RoleChangeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (roleId == Guid.Empty)
            {
                return BadRequest("Role id is required.");
            }
            if (!TryCreateRole(request, out _, out var error))
            {
                return BadRequest(error);
            }

            var role = new RoleModel(roleId, request.Description, request.IsActive);
            role.SetRoleName(request.Name);
            try
            {
                var result = await _authorizationApplication.UpdateRoleAsync(
                    role, request.PermissionIds, cancellationToken);
                return result == 0 ? BadRequest("Could not update role.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization operation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPut("accounts/{accountId:guid}/roles")]
        public async Task<IActionResult> UpdateAccountRolesAsync(
            [FromRoute] Guid accountId,
            [FromBody] IdListRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelFieldGuard.ValidateIds(accountId, request.Ids))
            {
                return BadRequest("Account id and at least one valid role id are required.");
            }

            try
            {
                var result = await _authorizationApplication.UpdateAccountRolesAsync(
                    accountId, request.Ids, cancellationToken);
                return result == 0 ? BadRequest("Could not update account roles.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization operation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPut("permissions/{permissionId:guid}")]
        public async Task<IActionResult> UpdatePermissionAsync(
            [FromRoute] Guid permissionId,
            [FromBody] PermissionChangeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (permissionId == Guid.Empty)
            {
                return BadRequest("Permission id is required.");
            }
            if (!TryCreatePermission(request, out _, out var error))
            {
                return BadRequest(error);
            }

            var permission = new PermissionModel(permissionId, request.Description, request.IsActive);
            permission.SetPermissionName(request.Name);
            try
            {
                var result = await _authorizationApplication.UpdatePermissionAsync(permission, cancellationToken);
                return result == 0 ? BadRequest("Could not update permission.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization operation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPut("accounts/{accountId:guid}/permissions")]
        public async Task<IActionResult> UpdateAccountPermissionsAsync(
            [FromRoute] Guid accountId,
            [FromBody] IdListRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelFieldGuard.ValidateIds(accountId, request.Ids))
            {
                return BadRequest("Account id and at least one valid permission id are required.");
            }

            try
            {
                var result = await _authorizationApplication.UpdateAccountAdditionalPermissionsAsync(
                    accountId, request.Ids, cancellationToken);
                return result == 0 ? BadRequest("Could not update account permissions.") : Ok(result);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Request canceled. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization operation failed.");
                return BadRequest($"An error occurred. {ex.Message}");
            }
        }

        #endregion

        #region PRIVATE

        private static bool TryCreateRole(
            RoleChangeRequest request,
            out RoleModel role,
            out string error)
        {
            role = null!;
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error = "Role name is required.";
                return false;
            }

            if (request.PermissionIds is not { Count: > 0 } ||
                request.PermissionIds.Any(id => id == Guid.Empty))
            {
                error = "At least one valid permission id is required.";
                return false;
            }

            role = new RoleModel(Guid.NewGuid(), request.Description, request.IsActive);
            role.SetRoleName(request.Name);
            return true;
        }

        private static bool TryCreatePermission(
            PermissionChangeRequest request,
            out PermissionModel permission,
            out string error)
        {
            permission = null!;
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error = "Permission name is required.";
                return false;
            }

            permission = new PermissionModel(Guid.NewGuid(), request.Description, request.IsActive);
            permission.SetPermissionName(request.Name);
            return true;
        }

        #endregion
    }
}
