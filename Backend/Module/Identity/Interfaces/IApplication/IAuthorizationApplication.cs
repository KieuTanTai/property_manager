using Identity.Models.Permission;
using Identity.Models.Role;
using Identity.Utils.Enum;

namespace Identity.Interfaces.IApplication
{
    public interface IAuthorizationApplication
    {
        #region Role

        Task<RoleModel> GetBaseRolesForUserAsync(CancellationToken cancellationToken = default);
        
        Task<RoleModel> GetRoleWithPermissionsAsync(ESystemRoleCode roleCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RoleModel>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default);

        Task<int> AddRoleAsync(RoleModel role, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);
        Task<int> AddAccountRolesAsync(Guid accountId, IReadOnlyList<Guid> roleIds, CancellationToken cancellationToken = default);

        Task<int> UpdateRoleAsync(RoleModel role, CancellationToken cancellationToken = default);
        
        Task<int> UpdateRoleAsync(RoleModel role, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);
        
        Task<int> UpdateAccountRolesAsync(Guid accountId, IReadOnlyList<Guid> roleIds, CancellationToken cancellationToken = default);

        #endregion

        #region Permission

        Task<PermissionModel> GetPermissionByCodeAsync(ESystemPermissionCode permissionCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PermissionModel>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
        Task<int> AddPermissionAsync(PermissionModel permissionModel, CancellationToken cancellationToken = default);
        Task<int> AddAccountAdditionalPermissionsAsync(Guid accountId, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);
        Task<int> UpdatePermissionAsync(PermissionModel permissionModel, CancellationToken cancellationToken = default);
        Task<int> UpdateAccountAdditionalPermissionsAsync(Guid accountId, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);

        #endregion
    }
}