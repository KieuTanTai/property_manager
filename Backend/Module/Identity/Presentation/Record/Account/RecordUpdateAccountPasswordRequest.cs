using System.ComponentModel.DataAnnotations;

namespace Identity.Presentation.Record.Account
{
    public record RecordUpdateAccountPasswordRequest(
        [Required]
        [EmailAddress]
        string Email,
        [Required]
        [DataType(DataType.Password)]
        string OldPassword,
        [Required]
        [DataType(DataType.Password)]
        string NewPassword
    );
}
