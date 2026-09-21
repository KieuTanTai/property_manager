using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace Identity.Presentation.Record.Profile
{
    public record RecordProfileResponse(
        [Required]
        string IdentityCode,
        [Required]
        Guid AccountId,
        string? FirstName = null,
        string? LastName = null,
        string? PhoneNumber = null,
        string? Address = null,
        string? AvatarUrl = null,
        DateTime? DateOfBirth = null,
        ESystemUserGender? Gender = null
    );
}