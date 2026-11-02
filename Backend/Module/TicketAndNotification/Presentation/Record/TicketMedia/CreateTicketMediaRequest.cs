using System.ComponentModel.DataAnnotations;

namespace TicketAndNotification.Presentation.Record.TicketMedia
{
    public sealed record CreateTicketMediaRequest(
        [Required]
        Guid TicketId,
        [Required]
        [MaxLength(410000)]
        string ImageUrl);
}
