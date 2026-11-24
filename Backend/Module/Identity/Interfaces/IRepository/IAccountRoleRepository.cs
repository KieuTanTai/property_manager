using Identity.Models.Account;
using Shared.Interfaces;

namespace Identity.Interfaces.IRepository
{
    public interface IAccountRoleRepository : IBaseAssociativeRepository<AccountRoleModel, Guid>
    {
        Task DeleteAsync(Guid accountId, Guid roleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AccountRoleModel>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AccountRoleModel>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    }
}
