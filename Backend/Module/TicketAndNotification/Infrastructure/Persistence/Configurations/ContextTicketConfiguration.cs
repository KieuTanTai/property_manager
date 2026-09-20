using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Infrastructure.Persistence.Configurations
{
    public sealed class ContextTicketConfiguration : IEntityTypeConfiguration<TicketModel>
    {
        public void Configure(EntityTypeBuilder<TicketModel> entity)
        {
            entity.ToTable("ticket");

            entity.HasKey(ticket => ticket.TicketId);

            entity.Property(ticket => ticket.TicketId)
                .HasColumnName("ticket_id")
                .ValueGeneratedOnAdd();

            entity.Property(ticket => ticket.TicketAccountId)
                .HasColumnName("ticket_account_id")
                .IsRequired();

            entity.Property(ticket => ticket.TicketContent)
                .HasColumnName("ticket_content")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(ticket => ticket.TicketType)
                .HasColumnName("ticket_type")
                .HasConversion<string>()
                .HasDefaultValue("feedback")
                .IsRequired();

            entity.Property(ticket => ticket.TicketIsResolved)
                .HasColumnName("ticket_is_resolved")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(ticket => ticket.TicketCreatedAt)
                .HasColumnName("ticket_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(ticket => ticket.TicketUpdatedAt)
                .HasColumnName("ticket_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasMany(ticket => ticket.TicketMedias)
                .WithOne()
                .HasForeignKey(ticketMedia => ticketMedia.TicketMediaTicketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
                