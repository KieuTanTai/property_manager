using Identity.Models.Role;
using Shared.Interfaces;

namespace Identity.Interfaces.IRepository
{
    public interface IRolePermissionRepository : IBaseAssociativeRepository<RolePermissionModel, Guid>
    {
        Task DeleteAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RolePermissionModel>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RolePermissionModel>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
    }
}