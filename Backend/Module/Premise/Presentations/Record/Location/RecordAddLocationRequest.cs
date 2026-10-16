using System.ComponentModel.DataAnnotations;

namespace Premise.Presentations.Record.Location
{
    public record RecordAddLocationRequest(
        [Required]
        [StringLength(255)]
        string LocationAddress
    );
}
