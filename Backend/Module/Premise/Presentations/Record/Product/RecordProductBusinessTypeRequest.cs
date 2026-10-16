using System.ComponentModel.DataAnnotations;

namespace Premise.Presentations.Record.Product
{
    public record RecordProductBusinessTypeRequest(
        [Required]
        Guid BusinessTypeId
    );
}
