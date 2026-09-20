using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace TicketAndNotification.Models.Notification
{
    public class NotificationModel
    {
        public NotificationModel(
            Guid senderAccountId,
            string content,
            ENotificationType type = ENotificationType.Other)
        {
            NotificationSenderAccountId = senderAccountId;
            NotificationContent = content;
            NotificationType = type;
        }

        public NotificationModel(
            Guid notificationId,
            Guid senderAccountId,
            ENotificationType type,
            string content,
            bool isRead)
        {
            NotificationId = notificationId;
            NotificationSenderAccountId = senderAccountId;
            NotificationType = type;
            NotificationContent = content;
            NotificationIsRead = isRead;
        }

        public NotificationModel() {}

        public Guid NotificationId { get; init; }

        public Guid NotificationSenderAccountId { get; private set; }

        public ENotificationType NotificationType { get; private set; } = ENotificationType.Other;

        [Required, MaxLength(255)]
        public string NotificationContent { get; private set; } = string.Empty;

        public bool NotificationIsRead { get; private set; }

        public DateTime NotificationCreatedAt { get; init; } = DateTime.Now;

        public DateTime NotificationUpdatedAt { get; private set; } = DateTime.Now;

        public IReadOnlyList<NotificationRecipientModel> NotificationRecipients { get; private set; } =
            new List<NotificationRecipientModel>();

        #region Setter

        public void SetNotificationSenderAccountId(Guid senderAccountId)
        {
            NotificationSenderAccountId = senderAccountId;
        }

        public void SetNotificationType(ENotificationType type)
        {
            NotificationType = type;
        }

        public void SetNotificationContent(string content)
        {
            NotificationContent = content;
        }

        public void SetNotificationIsRead(bool isRead)
        {
            NotificationIsRead = isRead;
        }

        public void SetNotificationRecipients(IReadOnlyList<NotificationRecipientModel> recipients)
        {
            NotificationRecipients = recipients;
        }

        #endregion
    }
}