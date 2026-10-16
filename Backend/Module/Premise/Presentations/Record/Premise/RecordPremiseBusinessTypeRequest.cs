using System.ComponentModel.DataAnnotations;

namespace Premise.Presentations.Record.Premise
{
    public record RecordPremiseBusinessTypeRequest(
        [Required]
        Guid BusinessTypeId
    );
}
