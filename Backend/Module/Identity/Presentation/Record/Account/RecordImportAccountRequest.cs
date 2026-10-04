using Identity.Models.Account;
using Identity.Models.Profile;

namespace Identity.Presentation.Record.Account
{
    public record RecordImportAccountRequest(
        IEnumerable<AccountModel> Accounts,
        IEnumerable<AccountRoleModel> AccountRoles,
        IEnumerable<UserProfileModel>? UserProfiles,
        IEnumerable<AccountAdditionalPermissionModel>? AccountAdditionalPermissions
        );
}