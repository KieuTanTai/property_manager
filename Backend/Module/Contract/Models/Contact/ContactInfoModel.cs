using Shared.ModelHelper;

namespace Contract.Models.Contact
{
    public class ContactInfoModel
    {
        public ContactInfoModel(string email, string phone, string address, string? socialLink = null)
        {
            ContactInfoEmail = ModelFieldGuard.Required(email, 255, nameof(email));
            ContactInfoPhone = ModelFieldGuard.Required(phone, 20, nameof(phone));
            ContactInfoAddress = ModelFieldGuard.Required(address, 255, nameof(address));
            ContactSocialLink = Optional(socialLink);
        }

        public ContactInfoModel(Guid contactInfoId, string email, string phone, string address, string? socialLink = null)
            : this(email, phone, address, socialLink)
        {
            ContactInfoId = contactInfoId;
        }

        public ContactInfoModel() { }

        public Guid ContactInfoId { get; init; }
        public string ContactInfoEmail { get; private set; } = string.Empty;
        public string ContactInfoPhone { get; private set; } = string.Empty;
        public string ContactInfoAddress { get; private set; } = string.Empty;
        public string? ContactSocialLink { get; private set; }
        public DateTime ContactInfoCreatedAt { get; init; } = DateTime.Now;
        public DateTime ContactInfoUpdatedAt { get; private set; } = DateTime.Now;

        #region Setter

        public void SetContactInfoEmail(string email) =>
            ContactInfoEmail = ModelFieldGuard.Required(email, 255, nameof(email));

        public void SetContactInfoPhone(string phone) =>
            ContactInfoPhone = ModelFieldGuard.Required(phone, 20, nameof(phone));

        public void SetContactInfoAddress(string address) =>
            ContactInfoAddress = ModelFieldGuard.Required(address, 255, nameof(address));

        public void SetContactSocialLink(string? socialLink) =>
            ContactSocialLink = Optional(socialLink);

        private static string? Optional(string? value)
        {
            var trimmed = value?.Trim();
            if (string.IsNullOrEmpty(trimmed)) return null;
            if (trimmed.Length > 255)
                throw new ArgumentOutOfRangeException(nameof(value), "Social link cannot exceed 255 characters.");
            return trimmed;
        }

        #endregion
    }
}
