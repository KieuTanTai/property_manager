using System.ComponentModel.DataAnnotations;

namespace Contract.Presentation.Record.Contact
{
    public sealed record RecordContactInfoRequest(
        [Required, EmailAddress, StringLength(255)] string ContactInfoEmail,
        [Required, StringLength(20)] string ContactInfoPhone,
        [Required, StringLength(255)] string ContactInfoAddress,
        [StringLength(255), Url] string? ContactSocialLink);
}
