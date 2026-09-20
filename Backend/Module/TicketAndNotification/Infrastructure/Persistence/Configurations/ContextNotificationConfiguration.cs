using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Enum;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Infrastructure.Persistence.Configurations
{
    public sealed class ContextNotificationConfiguration : IEntityTypeConfiguration<NotificationModel>
    {
        public void Configure(EntityTypeBuilder<NotificationModel> entity)
        {
            entity.ToTable("notification");

            entity.HasKey(notification => notification.NotificationId);

            entity.Property(notification => notification.NotificationId)
                .HasColumnName("notification_id")
                .ValueGeneratedOnAdd();

            entity.Property(notification => notification.NotificationSenderAccountId)
                .HasColumnName("notification_sender_account_id")
                .IsRequired();

            entity.Property(notification => notification.NotificationType)
                .HasColumnName("notification_type")
                .HasConversion<string>()
                .HasDefaultValue("other")
                .IsRequired();

            entity.Property(notification => notification.NotificationContent)
                .HasColumnName("notification_content")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(notification => notification.NotificationIsRead)
                .HasColumnName("notification_is_read")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(notification => notification.NotificationCreatedAt)
                .HasColumnName("notification_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(notification => notification.NotificationUpdatedAt)
                .HasColumnName("notification_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasMany(notification => notification.NotificationRecipients)
                .WithOne()
                .HasForeignKey(recipient => recipient.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}