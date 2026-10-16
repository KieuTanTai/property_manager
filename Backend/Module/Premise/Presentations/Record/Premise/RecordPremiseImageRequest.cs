using System.ComponentModel.DataAnnotations;

namespace Premise.Presentations.Record.Premise
{
    public record RecordPremiseImageRequest(
        [Required]
        [StringLength(255)]
        string ImageUrl
    );
}
