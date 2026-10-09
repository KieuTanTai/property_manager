using System.ComponentModel.DataAnnotations;

namespace TicketAndNotification.Presentation.Record.NotificationRecipient
{
    public sealed record CreateNotificationRecipientRequest(
        [Required]
        Guid NotificationId,
        [Required]
        Guid AccountId);
}
