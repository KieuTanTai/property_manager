using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Shared.Persistence.Record.Auth
{
    public record RecordClaimResponse(string Type, string Value);
    
    public record RecordAuthRequest(
        [Required]
        [EmailAddress]
        string Email,
        [Required]
        [DataType(DataType.Password)]
        string Password
        );
}