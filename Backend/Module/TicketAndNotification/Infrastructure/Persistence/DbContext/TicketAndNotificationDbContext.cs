using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using TicketAndNotification.Models.Notification;
using TicketAndNotification.Models.Ticket;
using MySqlModelBuilderExtensions=
    MySql.EntityFrameworkCore.Extensions.MySQLModelBuilderExtensions;

namespace TicketAndNotification.Infrastructure.Persistence.DbContext
{
    public sealed class TicketAndNotificationDbContext(
        DbContextOptions<TicketAndNotificationDbContext> options)
        : Microsoft.EntityFrameworkCore.DbContext(options)
    {
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<NotificationRecipientModel> NotificationRecipients { get; set; }
        public DbSet<TicketMediaModel> TicketMedias { get; set; }
        public DbSet<TicketModel> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasCharSet("utf8mb4");

            MySqlModelBuilderExtensions.UseCollation(
                modelBuilder,
                "utf8mb4_unicode_ci"
            );

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(TicketAndNotificationDbContext).Assembly
            );
        }
    }
}