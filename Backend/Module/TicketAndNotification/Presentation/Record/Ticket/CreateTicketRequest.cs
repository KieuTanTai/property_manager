using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace TicketAndNotification.Presentation.Record.Ticket
{
    public sealed record CreateTicketRequest(
        [Required]
        Guid AccountId,
        [Required]
        [MaxLength(255)]
        string Content,
        ETicketType Type = ETicketType.Feedback);
}
