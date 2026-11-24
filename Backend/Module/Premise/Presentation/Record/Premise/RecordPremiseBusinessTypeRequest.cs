using System.ComponentModel.DataAnnotations;

namespace Premise.Presentation.Record.Premise
{
    public record RecordPremiseBusinessTypeRequest(
        [Required]
        Guid BusinessTypeId
    );
}