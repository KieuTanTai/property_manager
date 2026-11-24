using System.ComponentModel.DataAnnotations;

namespace Premise.Presentation.Record.Location
{
    public record RecordAddLocationRequest(
        [Required]
        [StringLength(255)]
        string LocationAddress
    );
}