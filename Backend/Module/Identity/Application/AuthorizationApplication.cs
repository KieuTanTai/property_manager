using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Identity.Models.Permission;
using Identity.Models.Role;
using Shared.Enum;
using Shared.Interfaces;
using Shared.Logging;

namespace Identity.Application
{
    public class AuthorizationApplication(
        IUnitOfWork unitOfWork,
        IBaseAuthorizationRepository<RoleModel, ESystemRoleCode, Guid> roleRepository,
        IBaseAuthorizationRepository<PermissionModel, ESystemPermissionCode, Guid> permissionRepository,
        IBaseAssociativeRepository<RolePermissionModel, Guid> rolePermissionRepository,
        IBaseAssociativeRepository<AccountAdditionalPermissionModel, Guid> accountAdditionalPermissionRepository,
        IBaseAssociativeRepository<AccountRoleModel, Guid> accountRoleRepository,
        ILogger<AuthorizationApplication> logger,
        ILogPool logPool) : IAuthorizationApplication
    {
        private const string Module = "Identity";

        private const string Layer = "Application";

        private readonly IBaseAssociativeRepository<AccountAdditionalPermissionModel, Guid> _accountAdditionalPermissionRepository = accountAdditionalPermissionRepository;


        private readonly IBaseAssociativeRepository<AccountRoleModel, Guid> _accountRoleRepository = accountRoleRepository;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<AuthorizationApplication> _logger = logger;

        private readonly IBaseAuthorizationRepository<PermissionModel, ESystemPermissionCode, Guid> _permissionRepository = permissionRepository;

        private readonly IBaseAssociativeRepository<RolePermissionModel, Guid> _rolePermissionRepository = rolePermissionRepository;

        private readonly IBaseAuthorizationRepository<RoleModel, ESystemRoleCode, Guid> _roleRepository = roleRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        #region Private

        private static async Task<(List<TAssociativeModel> ModelsToRemove, List<Guid> IdsToAdd)> UpdateAssociativeTableAsync<TAssociativeModel>(
            Guid id,
            IReadOnlyList<Guid> ids,
            Func<Guid, CancellationToken, Task<IReadOnlyList<TAssociativeModel>>> getExistingAssociativeModelsFunc,
            Func<TAssociativeModel, bool> shouldRemoveFunc,
            Func<Guid, TAssociativeModel, bool> isSameAssociationFunc,
            CancellationToken cancellationToken)
        {
            var existingAssociativeModels =
                await getExistingAssociativeModelsFunc(id, cancellationToken);

            var modelsToRemove = existingAssociativeModels
                .Where(shouldRemoveFunc)
                .ToList();

            var idsToAdd = ids
                .Where(guid => existingAssociativeModels
                    .All(model => !isSameAssociationFunc(guid, model)))
                .ToList();

            return (modelsToRemove, idsToAdd);
        }

        #endregion

        #region Role

        #region Get

        public async Task<RoleModel> GetBaseRolesForUserAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Base role lookup operation started.");
            try
            {
                var result = await _roleRepository.GetByCodeAsync(ESystemRoleCode.Customer, cancellationToken);
                return result ?? throw new Exception("Role not found.");
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Base role lookup operation failed.");
                throw new Exception("Failed to fetch base role for user.", ex);
            }
        }

        public async Task<RoleModel> GetRoleWithPermissionsAsync(ESystemRoleCode roleCode, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Role permission lookup operation started.");
            try
            {
                var role = await _roleRepository.GetByCodeAsync(roleCode, cancellationToken);
                return role ?? throw new Exception($"Role with code {roleCode} not found.");
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Role permission lookup operation failed.");
                throw new Exception($"Failed to fetch permissions for role {roleCode}.", ex);
            }
        }

        public async Task<IReadOnlyList<RoleModel>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Role list operation started.");
            try
            {
                var roles = await _roleRepository.GetAllAsync(cancellationToken);
                if (roles == null || !roles.Any())
                {
                    throw new Exception("No roles found.");
                }
                return roles;
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Role list operation failed.");
                throw new Exception("Failed to fetch roles with permissions.", ex);
            }
        }

        #endregion

        #region Add

        public async Task<int> AddRoleAsync(RoleModel roleModel, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Role creation operation started.");
            try
            {
                await _roleRepository.AddAsync(roleModel, cancellationToken);
                foreach (var permissionId in permissionIds)
                {
                    var rolePermission = new RolePermissionModel(roleModel.RoleId, permissionId);
                    await _rolePermissionRepository.AddAsync(rolePermission, cancellationToken);
                }
                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to add role {roleModel.RoleName} with permissions.", ex);
            }
        }

        public async Task<int> AddAccountRolesAsync(Guid accountId, IReadOnlyList<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account role assignment operation started.");
            try
            {
                foreach (var roleId in roleIds)
                {
                    var accountRole = new AccountRoleModel(accountId, roleId);
                    await _accountRoleRepository.AddAsync(accountRole, cancellationToken);
                }
                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to add roles to account {accountId}.", ex);
            }
        }

        #endregion

        #region Update

        public async Task<int> UpdateRoleAsync(RoleModel roleModel, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Role update operation started.");
            try
            {
                if (roleModel.RoleCode == nameof(ESystemRoleCode.Admin))
                {
                    throw new ArgumentException("Cannot update admin roles.", nameof(roleModel));
                }
                await _roleRepository.UpdateAsync(roleModel, cancellationToken);
                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to update role {roleModel.RoleName}.", ex);
            }
        }

        public async Task<int> UpdateRoleAsync(RoleModel roleModel, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Role permission update operation started.");
            try
            {
                var (permissionsToRemove, permissionsToAdd) = await UpdateAssociativeTableAsync(
                    roleModel.RoleId,
                    permissionIds,
                    _rolePermissionRepository.GetBySecondForeignIdAsync,
                    association => !permissionIds.Contains(association.PermissionId),
                    (permissionId, association) => permissionId == association.PermissionId,
                    cancellationToken);

                foreach (var permissionToRemove in permissionsToRemove)
                    await _rolePermissionRepository.DeleteAsync(permissionToRemove.RoleId, permissionToRemove.PermissionId, cancellationToken);
                foreach (var permission in permissionsToAdd.Select(permissionToAdd => new RolePermissionModel(roleModel.RoleId, permissionToAdd)))
                    await _rolePermissionRepository.AddAsync(permission, cancellationToken);

                await _roleRepository.UpdateAsync(roleModel, cancellationToken);
                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to update role {roleModel.RoleCode} with permissions.", ex);
            }
        }

        public async Task<int> UpdateAccountRolesAsync(Guid accountId, IReadOnlyList<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account role update operation started.");
            try
            {
                var (rolesToRemove, rolesToAdd) = await UpdateAssociativeTableAsync(
                    accountId,
                    roleIds,
                    _accountRoleRepository.GetByFirstForeignIdAsync,
                    association => !roleIds.Contains(association.RoleId),
                    (roleId, association) => roleId == association.RoleId,
                    cancellationToken);

                foreach (var roleToRemove in rolesToRemove)
                    await _accountRoleRepository.DeleteAsync(roleToRemove.AccountId, roleToRemove.RoleId, cancellationToken);
                foreach (var role in rolesToAdd.Select(roleToAdd => new AccountRoleModel(accountId, roleToAdd)))
                    await _accountRoleRepository.AddAsync(role, cancellationToken);

                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to update account {accountId} with roles.", ex);
            }
        }

        #endregion

        #endregion

        #region Permission

        #region Get

        public async Task<PermissionModel> GetPermissionByCodeAsync(ESystemPermissionCode permissionCode, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Permission lookup operation started.");
            try
            {
                var permission = await _permissionRepository.GetByCodeAsync(permissionCode, cancellationToken);
                return permission ?? throw new Exception($"Permission with code {permissionCode} not found.");
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to fetch permission with code {permissionCode}.", ex);
            }
        }

        public async Task<IReadOnlyList<PermissionModel>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Permission list operation started.");
            try
            {
                var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
                if (permissions == null || !permissions.Any())
                {
                    throw new Exception("No permissions found.");
                }
                return permissions;
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception("Failed to fetch all permissions.", ex);
            }
        }

        #endregion

        #region Add

        public async Task<int> AddPermissionAsync(PermissionModel permissionModel, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Permission creation operation started.");
            try
            {
                await _permissionRepository.AddAsync(permissionModel, cancellationToken);
                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to add permission {permissionModel.PermissionCode}.", ex);
            }
        }

        public async Task<int> AddAccountAdditionalPermissionsAsync(Guid accountId, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account permission assignment operation started.");
            try
            {
                foreach (var permissionId in permissionIds)
                {
                    var accountPermission = new AccountAdditionalPermissionModel(accountId, permissionId);
                    await _accountAdditionalPermissionRepository.AddAsync(accountPermission, cancellationToken);
                }
                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to add additional permissions to account {accountId}.", ex);
            }
        }

        #endregion

        #region Update

        public async Task<int> UpdatePermissionAsync(PermissionModel permissionModel, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Permission update operation started.");
            try
            {
                await _permissionRepository.UpdateAsync(permissionModel, cancellationToken);
                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to update permission {permissionModel.PermissionCode}.", ex);
            }
        }

        public async Task<int> UpdateAccountAdditionalPermissionsAsync(Guid accountId, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account permission update operation started.");
            try
            {
                var (modelsToRemove, idsToAdd) =
                    await UpdateAssociativeTableAsync(
                        accountId,
                        permissionIds,
                        _accountAdditionalPermissionRepository.GetBySecondForeignIdAsync,
                        association => !permissionIds.Contains(association.PermissionId),
                        (permissionId, association) => permissionId == association.PermissionId,
                        cancellationToken);

                foreach (var permissionToRemove in modelsToRemove)
                    await _accountAdditionalPermissionRepository.DeleteAsync(permissionToRemove.AccountId, permissionToRemove.PermissionId, cancellationToken);
                foreach (var permission in idsToAdd.Select(permissionToAdd => new AccountAdditionalPermissionModel(accountId, permissionToAdd)))
                    await _accountAdditionalPermissionRepository.AddAsync(permission, cancellationToken);

                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Authorization application operation failed.");
                throw new Exception($"Failed to update account {accountId} with additional permissions.", ex);
            }
        }

        #endregion

        #endregion
    }
}
