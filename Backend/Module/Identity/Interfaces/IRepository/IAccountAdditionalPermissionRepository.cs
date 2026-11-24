using Identity.Models.Account;
using Shared.Interfaces;

namespace Identity.Interfaces.IRepository
{
    public interface IAccountAdditionalPermissionRepository : IBaseAssociativeRepository<AccountAdditionalPermissionModel, Guid>
    {
        Task DeleteAsync(Guid accountId, Guid permissionId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AccountAdditionalPermissionModel>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AccountAdditionalPermissionModel>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
    }
}
