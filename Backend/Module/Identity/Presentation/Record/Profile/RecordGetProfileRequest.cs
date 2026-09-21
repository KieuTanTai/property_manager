using System.ComponentModel.DataAnnotations;

namespace Identity.Presentation.Record.Profile
{
    public record RecordGetProfileRequest(
        Guid? AccountId,
        [MaxLength(12)]
        string? IdentityCode);
}