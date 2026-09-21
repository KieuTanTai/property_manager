using System.ComponentModel.DataAnnotations;
using Identity.Models.Permission;
using Identity.Models.Profile;
using Identity.Models.Role;
using Shared.ModelHelper;

namespace Identity.Models.Account
{
    public class AccountModel
    {
        public AccountModel(string accountEmail, string password, bool accountIsActive, bool isManualGenerateId = false)
        {
            if (isManualGenerateId)
            {
                AccountId = Guid.CreateVersion7();
            }
            AccountEmail = ModelFieldGuard.Required(accountEmail, 255, nameof(accountEmail));
            AccountPassword = ModelFieldGuard.Required(password, 255, nameof(password));
            AccountIsActive = accountIsActive;
        }

        public AccountModel(string email, string password)
        {
            AccountEmail = ModelFieldGuard.Required(email, 255, nameof(email));
            AccountPassword = ModelFieldGuard.Required(password, 255, nameof(password));
        }

        public AccountModel(string accountEmail, string? accountPassword, bool accountIsActive)
        {
            AccountEmail = ModelFieldGuard.Required(accountEmail, 255, nameof(accountEmail));
            AccountPassword = ModelFieldGuard.Required(accountPassword, 255, nameof(accountPassword));
            AccountIsActive = accountIsActive;
        }

        public AccountModel(Guid accountId, string? accountEmail, string? accountPassword, bool accountIsActive)
        {
            AccountId = accountId;
            AccountEmail = accountEmail;
            AccountPassword = accountPassword;
            AccountIsActive = accountIsActive;
        }

        public AccountModel(Guid accountId, bool accountIsActive)
        {
            AccountId = accountId;
            AccountIsActive = accountIsActive;
        }

        public AccountModel() {}

        public Guid AccountId { get; init; }

        [MaxLength(255)] public string? AccountEmail { get; private set; } = string.Empty;

        [MaxLength(255)] public string? AccountPassword { get; private set; } = string.Empty;

        public bool AccountIsActive { get; private set; } = true;

        public DateTime AccountCreatedAt { get; init; } = DateTime.Now;

        public DateTime AccountUpdatedAt { get; private set; } = DateTime.Now;

        public IReadOnlyList<RoleModel> Roles { get; private set; } = new List<RoleModel>();

        public IReadOnlyList<PermissionModel> Permissions { get; private set; } =
            new List<PermissionModel>();

        public UserProfileModel? UserProfile { get; private set; }


        #region Setter

        public void SetEmail(string email)
        {
            AccountEmail = ModelFieldGuard.Required(email, 255, nameof(email));
        }

        public void SetAccountIsActive(bool isActive)
        {
            if (AccountIsActive == isActive)
            {
                return;
            }
            AccountIsActive = isActive;
            AccountIsActive = isActive;
        }

        public void SetHashedPassword(string passwordHash)
        {
            AccountPassword = ModelFieldGuard.Required(passwordHash, 255, nameof(passwordHash));
        }

        public void SetRoles(IReadOnlyList<RoleModel> roles)
        {
            Roles = roles;
        }

        public void SetPermissions(IReadOnlyList<PermissionModel> permissions)
        {
            Permissions = permissions;
        }

        public void SetUserProfile(UserProfileModel userProfile)
        {
            UserProfile = userProfile;
        }

        #endregion
    }
}