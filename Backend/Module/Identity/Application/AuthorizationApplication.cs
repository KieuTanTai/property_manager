using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Models.Permission;
using Identity.Models.Role;
using Identity.Utils.Enum;
using Shared.Interfaces;
using StackExchange.Redis;

namespace Identity.Application
{
    public class AuthorizationApplication(
        IUnitOfWork unitOfWork,
        IBaseAuthorizationRepository<RoleModel, ESystemRoleCode, Guid> roleRepository,
        IBaseAuthorizationRepository<PermissionModel, ESystemPermissionCode, Guid> permissionRepository,
        IBaseAssociativeRepository<RolePermissionModel, Guid> rolePermissionRepository,
        IAccountRepository accountRepository) : IAuthorizationApplication
    {
        private readonly IAccountRepository _accountRepository = accountRepository;
        
        private readonly IBaseAuthorizationRepository<PermissionModel, ESystemPermissionCode, Guid> _permissionRepository = permissionRepository;
        
        private readonly IBaseAuthorizationRepository<RoleModel, ESystemRoleCode, Guid> _roleRepository = roleRepository;

        private readonly IBaseAssociativeRepository<RolePermissionModel, Guid> _rolePermissionRepository = rolePermissionRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        #region USER
        
        public async Task<RoleModel> GetBaseRolesForUserAsync(CancellationToken cancellationToken = default)
        {
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
                // Log the exception
                throw new Exception("Failed to fetch base role for user.", ex);
            }
        }

        public async Task<RoleModel> GetRoleWithPermissionsAsync(ESystemRoleCode roleCode, CancellationToken cancellationToken = default)
        {
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
                // Log the exception
                throw new Exception($"Failed to fetch permissions for role {roleCode}.", ex);
            }
        }

        public async Task<IReadOnlyList<RoleModel>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync(cancellationToken);
                if (roles == null || !roles.Any())
                    throw new Exception("No roles found.");
                return roles;
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("Failed to fetch roles with permissions.", ex);
            }
        }
        
        #endregion

        #region UPDATE

        public async Task<int> UpdateRoleAsync(RoleModel roleModel, CancellationToken cancellationToken = default)
        {
            if (roleModel.RoleId == Guid.Empty)
                throw new ArgumentException("RoleModel id is required.", nameof(roleModel.RoleId));
            if (string.IsNullOrWhiteSpace(roleModel.RoleName))
                throw new ArgumentException("RoleModel name is required.", nameof(roleModel.RoleName));
            
            try
            {
                await _roleRepository.UpdateAsync(roleModel, cancellationToken);
                return await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException canceledException)
            {
                throw new OperationCanceledException("Operation was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Failed to update role {roleModel.RoleName}.", ex);
            }
        }

        public async Task<int> UpdateRoleAsync(RoleModel roleModel, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            if (roleModel.RoleId == Guid.Empty)
                throw new ArgumentException("RoleModel id is required.", nameof(roleModel.RoleId));
            if (string.IsNullOrWhiteSpace(roleModel.RoleName))
                throw new ArgumentException("RoleModel name is required.", nameof(roleModel.RoleName));
            if (permissionIds == null || !permissionIds.Any())
                throw new ArgumentException("Permission ids are required.", nameof(permissionIds));

            try
            {
                var existingRolePermissions = await _rolePermissionRepository.GetBySecondForeignIdAsync(roleModel.RoleId, cancellationToken);
                var permissionsToRemove = existingRolePermissions.Where(rp => !permissionIds.Contains(rp.PermissionId)).ToList();
                var permissionsToAdd = permissionIds.Where(pid => existingRolePermissions.All(rp => rp.PermissionId != pid)).ToList();
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
                // Log the exception
                throw new Exception($"Failed to update role {roleModel.RoleCode} with permissions.", ex);
            }
        }
        
        #endregion
        
    }
}