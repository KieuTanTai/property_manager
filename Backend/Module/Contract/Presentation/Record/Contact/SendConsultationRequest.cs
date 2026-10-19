using System.ComponentModel.DataAnnotations;

namespace Contract.Presentation.Record.Contact
{
    public sealed record SendConsultationRequest(
        [Required] [StringLength(100)] [RegularExpression(@"^[^\d]*$", ErrorMessage = "Name must not contain numbers.")] string Name,
        [Required] [StringLength(20)] string Phone,
        [Required] [StringLength(255)] string SenderEmail,
        [StringLength(100)] string? PremiseName,
        [StringLength(2000)] string? Message);
}
