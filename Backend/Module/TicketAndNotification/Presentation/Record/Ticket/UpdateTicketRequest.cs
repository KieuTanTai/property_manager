using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace TicketAndNotification.Presentation.Record.Ticket
{
    public sealed record UpdateTicketRequest(
        [Required]
        Guid TicketId,
        [Required]
        Guid AccountId,
        [Required]
        [MaxLength(255)]
        string Content,
        ETicketType Type,
        bool IsResolved);
}
