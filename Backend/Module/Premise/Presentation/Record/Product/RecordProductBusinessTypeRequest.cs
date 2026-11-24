using System.ComponentModel.DataAnnotations;

namespace Premise.Presentation.Record.Product
{
    public record RecordProductBusinessTypeRequest(
        [Required]
        Guid BusinessTypeId
    );
}