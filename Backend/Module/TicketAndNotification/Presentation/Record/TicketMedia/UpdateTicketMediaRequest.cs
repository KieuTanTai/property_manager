using System.ComponentModel.DataAnnotations;

namespace TicketAndNotification.Presentation.Record.TicketMedia
{
    public sealed record UpdateTicketMediaRequest(
        [Required]
        int TicketMediaId,
        [Required]
        Guid TicketId,
        [Required]
        [MaxLength(255)]
        string ImageUrl);
}
