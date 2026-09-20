using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Infrastructure.Persistence.Configurations
{
    public sealed class ContextNotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipientModel>
    {
        public void Configure(EntityTypeBuilder<NotificationRecipientModel> entity)
        {
            entity.ToTable("notification_recipient");

            entity.HasKey(notificationRecipient => new{
                notificationRecipient.NotificationId,
                notificationRecipient.AccountId
            });

            entity.Property(notificationRecipient => notificationRecipient.NotificationId)
                .HasColumnName("nr_notification_id")
                .IsRequired();

            entity.Property(notificationRecipient => notificationRecipient.AccountId)
                .HasColumnName("nr_account_id")
                .IsRequired();

            entity.HasOne<NotificationModel>()
                .WithMany(notification => notification.NotificationRecipients)
                .HasForeignKey(notificationRecipient => notificationRecipient.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}