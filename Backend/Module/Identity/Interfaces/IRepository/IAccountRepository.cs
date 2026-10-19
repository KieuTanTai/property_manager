using Identity.Models.Account;
using Identity.Utils.Enum;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Identity.Interfaces.IRepository
{
    public interface IAccountRepository : IBaseReadRepository<AccountModel, Guid>, IBaseUpsertRepository<AccountModel>
    {
        Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize, bool isGetProfile, CancellationToken cancellationToken = default);

        Task<AccountModel> GetAccountByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<AccountModel> GetTrackedAccountByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize, bool isActive,
            CancellationToken cancellationToken = default);

        Task<AccountModel?> GetAccountAndNavigationByEmailAsync(string email, bool isGetRole = true, bool isGetAdditionalPermission = true, bool isGetProfile = false,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AccountModel>> GetAllAccountsByRoleCodeAsync(ESystemRoleCode roleCode, CancellationToken cancellationToken = default);

    }
}
