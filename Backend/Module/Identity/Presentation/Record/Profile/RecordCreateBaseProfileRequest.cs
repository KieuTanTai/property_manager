using System.ComponentModel.DataAnnotations;

namespace Identity.Presentation.Record.Profile
{
    public record RecordCreateBaseProfileRequest(
        [Required]
        Guid AccountId,
        [Required]
        [RegularExpression("^\\d{12}$", ErrorMessage = "Identity code must contain exactly 12 digits.")]
        [DataType(DataType.Text)]
        string IdentityCode);
}
