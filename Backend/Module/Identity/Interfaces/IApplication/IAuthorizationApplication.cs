using Identity.Models.Role;
using Identity.Utils.Enum;

namespace Identity.Interfaces.IApplication
{
    public interface IAuthorizationApplication
    {
        Task<RoleModel> GetBaseRolesForUserAsync(CancellationToken cancellationToken = default);
        
        Task<RoleModel> GetRoleWithPermissionsAsync(ESystemRoleCode roleCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RoleModel>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateRoleAsync(RoleModel role, CancellationToken cancellationToken = default);
        
        Task<int> UpdateRoleAsync(RoleModel role, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);
        
    }
}