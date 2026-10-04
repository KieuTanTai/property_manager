using System.ComponentModel.DataAnnotations;

namespace Identity.Presentation.Record.Account
{
    public record RecordInactiveAccountRequest(
        [Required]
        [EmailAddress]
        string Email,
        [Required]
        [DataType(DataType.Password)]
        string Password,
        [Required]
        [DataType(DataType.Password)]
        [property: Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        string ConfirmPassword
    );
}