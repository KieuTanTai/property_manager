using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace TicketAndNotification.Presentation.Record.Notification
{
    public sealed record CreateNotificationRequest(
        [Required]
        Guid SenderAccountId,
        [Required]
        [MaxLength(255)]
        string Content,
        ENotificationType Type = ENotificationType.Other);
}
