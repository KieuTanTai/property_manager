using System.ComponentModel.DataAnnotations;
using Shared.Enum;
using Shared.ModelHelper;

namespace Premise.Presentation.Record.Premise
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
        [RegularExpression(ModelFieldGuard.PremiseAreaPattern, ErrorMessage = "Diện tích chỉ được chứa chữ số và tối đa một dấu thập phân.")]
        string PremiseArea,
        [StringLength(100)]
        string? PremiseDescription
    );
}