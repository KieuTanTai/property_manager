namespace TicketAndNotification.Models.Notification
{
    public class NotificationRecipientModel
    {
        public NotificationRecipientModel(Guid notificationId, Guid accountId)
        {
            NotificationId = notificationId;
            AccountId = accountId;
        }

        public NotificationRecipientModel() {}

        public Guid NotificationId { get; init; }

        public Guid AccountId { get; init; }
    }
}
