using System.ComponentModel.DataAnnotations;

namespace Premise.Presentations.Record.Location
{
    public record RecordUpdateLocationRequest(
        [Required]
        [StringLength(255)]
        string LocationAddress
    );
}
