using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace Premise.Presentations.Record.Premise
{
    public record RecordAddPremiseRequest(
        [Required]
        [StringLength(50)]
        string PremiseName,

        [Required]
        Guid PremiseLocationId,

        EPremiseStatus PremiseStatus,

        int PremisePosition,

        int PremiseFloor,

        [Required]
        [StringLength(10)]
        string PremiseArea,

        [StringLength(100)]
        string? PremiseDescription
    );
}
