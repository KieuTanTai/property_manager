using System.ComponentModel.DataAnnotations;
using Shared.Enum;
using Shared.ModelHelper;

namespace Identity.Models.Profile
{
    public class UserProfileModel
    {
        public UserProfileModel(string userProfileId, Guid userProfileAccountId, string? userProfileFirstName, string? userProfileLastName, DateTime? userProfileDateOfBirth, ESystemUserGender userProfileGender,
            string? userProfilePhoneNumber, string? userProfileAddress, string? userProfileAvatarUrl)
        {
            UserProfileId = userProfileId ?? throw new ArgumentNullException(nameof(userProfileId));
            if (string.IsNullOrWhiteSpace(UserProfileId) || UserProfileId.Length != 12)
            {
                throw new ArgumentException("User Profile Id must be 12 characters long.", nameof(UserProfileId));
            }
            UserProfileAccountId = userProfileAccountId;
            UserProfileFirstName = userProfileFirstName;
            UserProfileLastName = userProfileLastName;
            UserProfileDateOfBirth =userProfileDateOfBirth;
            UserProfileGender = userProfileGender;
            UserProfilePhoneNumber = userProfilePhoneNumber;
            UserProfileAddress = userProfileAddress;
            UserProfileAvatarUrl = userProfileAvatarUrl;
        }

        public UserProfileModel(string userProfileId, Guid userProfileAccountId)
        {
            UserProfileId = userProfileId ?? throw new ArgumentNullException(nameof(userProfileId));
            if (string.IsNullOrWhiteSpace(UserProfileId) || UserProfileId.Length != 12)
            {
                throw new ArgumentException("User Profile Id must be 12 characters long.", nameof(UserProfileId));
            }
            UserProfileAccountId = userProfileAccountId;
        }

        [Required]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "User Profile Id must be 12 characters long.")]
        public string UserProfileId { get; private set; }

        public Guid UserProfileAccountId { get; init; }

        [MaxLength(30)]
        public string? UserProfileFirstName { get; private set; } = "";

        [MaxLength(30)]
        public string? UserProfileLastName { get; private set; } = "";

        public DateTime? UserProfileDateOfBirth { get; private set; }
        public ESystemUserGender UserProfileGender { get; private set; }

        [MaxLength(10)]
        public string? UserProfilePhoneNumber { get; private set; } = "";

        [MaxLength(255)]
        public string? UserProfileAddress { get; private set; } = "";

        [MaxLength(255)]
        public string? UserProfileAvatarUrl { get; private set; } = "";

        public DateTime UserProfileCreatedAt { get; init; } = DateTime.Now;
        public DateTime UserProfileUpdatedAt { get; private set; } = DateTime.Now;

        #region SET

        public void SetUserProfileId(string id)
        {
            UserProfileId = ModelFieldGuard.Required(id, 12, nameof(id));
        }


        public void SetUserProfileFirstName(string firstName)
        {
            UserProfileFirstName = ModelFieldGuard.Required(firstName, 30, nameof(firstName));
        }

        public void SetUserProfileLastName(string lastName)
        {
            UserProfileLastName = ModelFieldGuard.Required(lastName, 30, nameof(lastName));
        }

        public void SetUserProfileBirthday(DateTime birthday)
        {
            UserProfileDateOfBirth = birthday;
        }

        public void SetUserProfileGender(ESystemUserGender gender)
        {
            UserProfileGender = gender;
        }

        public void SetUserProfilePhoneNumber(string phoneNumber)
        {
            UserProfilePhoneNumber = ModelFieldGuard.Required(phoneNumber, 10, nameof(phoneNumber));
        }

        public void SetUserProfileAvatar(string avatar)
        {
            UserProfileAvatarUrl = ModelFieldGuard.Required(avatar, 255, nameof(avatar));
        }

        public void SetUserProfileAddress(string address)
        {
            UserProfileAddress = ModelFieldGuard.Required(address, 255, nameof(address));
        }

        #endregion
    }
}