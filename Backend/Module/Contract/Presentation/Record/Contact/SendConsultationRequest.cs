using System.ComponentModel.DataAnnotations;

namespace Contract.Presentation.Record.Contact
{
    public sealed record SendConsultationRequest(
        [Required, StringLength(100)] string Name,
        [Required, StringLength(20)] string Phone,
        [StringLength(100)] string? PremiseName,
        [StringLength(2000)] string? Message);
}
