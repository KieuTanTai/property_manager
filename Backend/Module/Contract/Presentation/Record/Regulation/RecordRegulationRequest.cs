using System.ComponentModel.DataAnnotations;

namespace Contract.Presentation.Record.Regulation
{
    public sealed record RecordRegulationRequest(
        [Required]
        [StringLength(50, MinimumLength = 1)]
        string RegulationName,
        [StringLength(255)]
        string? RegulationDescription,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        decimal? RegulationFineAmount,
        bool RegulationIsActive = true);
}