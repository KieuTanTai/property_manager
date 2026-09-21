using Identity.Interfaces;
using Identity.Models.Account;
using Identity.Models.Profile;
using Identity.Presentation.Record.Profile;
using Shared.Persistence.Record.Auth;

namespace Identity.Utils
{
    public class ApiHelper : IApiHelper
    {
        public RecordAuthResponse MappingAuthResult(AccountModel result)
        {
            var roleCodes = result.Roles.Select(role => role.RoleCode).ToList();
            var additionalPermissionCodes = result.AdditionalPermissions.Select(permission => permission.PermissionCode).ToList();
            return new RecordAuthResponse(result.AccountId, result.AccountEmail!, result.AccountIsActive, roleCodes, additionalPermissionCodes, result.AccountCreatedAt, result.AccountUpdatedAt);
        }

        public RecordProfileResponse MappingProfileResult(UserProfileModel model)
        {
            return new RecordProfileResponse(model.UserProfileId, model.UserProfileAccountId, model.UserProfileFirstName,
                model.UserProfileLastName, model.UserProfilePhoneNumber, model.UserProfileAddress, model.UserProfileAvatarUrl, model.UserProfileDateOfBirth, model.UserProfileGender);
        }
    }
}