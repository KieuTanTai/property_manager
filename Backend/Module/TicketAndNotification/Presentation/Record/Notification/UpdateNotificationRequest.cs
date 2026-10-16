using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace TicketAndNotification.Presentation.Record.Notification
{
    public sealed record UpdateNotificationRequest(
        [Required]
        Guid NotificationId,
        [Required]
        Guid SenderAccountId,
        ENotificationType Type,
        [Required]
        [MaxLength(255)]
        string Content,
        bool IsRead);
}
