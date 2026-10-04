using Identity.Models.Account;
using Identity.Models.Profile;
using Shared.Persistence.Record;

namespace Identity.Interfaces.IApplication
{
    public interface IAccountApplication
    {
        Task<AccountModel> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<AccountModel> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<bool> LogoutAsync(string email, CancellationToken cancellationToken = default);
        Task<int> ChangePasswordAsync(string email, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
        Task<int> InactiveAccountAsync(string email, string password, CancellationToken cancellationToken = default);

        // Methods for admins
        Task<int> ActiveAccountByAdminAsync(string email, CancellationToken cancellationToken = default);
        Task<int> InactiveAccountByAdminAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize, bool isGetProfile, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize, bool isActive, CancellationToken cancellationToken = default);
        Task<AccountModel?> GetAccountByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<int> ImportAsync(IEnumerable<AccountModel> entities, IEnumerable<AccountRoleModel> accountRoleModels, 
            IEnumerable<AccountAdditionalPermissionModel>? accountAdditionalPermissionModels, IEnumerable<UserProfileModel>? userProfiles, CancellationToken cancellationToken = default);
        Task<int> UpdateRange(IEnumerable<AccountModel> entities, CancellationToken cancellationToken = default);
    }
}