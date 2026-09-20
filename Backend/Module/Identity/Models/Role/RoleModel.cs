using System.ComponentModel.DataAnnotations;
using Identity.Models.Permission;
using Shared.ModelHelper;

namespace Identity.Models.Role
{
    public class RoleModel(Guid roleId, string? roleDescription, bool roleIsActive)
    {
        public Guid RoleId { get; init; } = roleId;

        [MaxLength(50)] public string RoleCode { get; private set; } = string.Empty;

        [MaxLength(150)] public string RoleName { get; private set; } = string.Empty;

        [MaxLength(300)] public string? RoleDescription { get; private set; } = roleDescription;

        public bool RoleIsActive { get; private set; } = roleIsActive;

        public DateTime RoleCreatedAt { get; init; } = DateTime.Now;

        public DateTime RoleUpdatedAt { get; private set; } = DateTime.Now;

        public IReadOnlyList<PermissionModel> Permissions { get; private set; } =
            new List<PermissionModel>();

        #region Setter

        public void SetRoleName(string name)
        {
            RoleName = ModelFieldGuard.Required(name, 150, nameof(name));
        }

        public void SetRoleIsActive(bool isActive)
        {
            if (RoleIsActive == isActive)
            {
                return;
            }
            RoleIsActive = isActive;
            RoleIsActive = isActive;
        }

        public void ClearRoleDescription()
        {
            if (RoleDescription is null)
            {
                return;
            }
            RoleDescription = null;
            RoleDescription = null;
        }

        public void SetRoleDescription(string description)
        {
            RoleDescription = ModelFieldGuard.Required(description, 300, nameof(description));
        }

        public void SetPermissions(IReadOnlyList<PermissionModel> permissions)
        {
            Permissions = permissions;
        }

        #endregion
    }
}