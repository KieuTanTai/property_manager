using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Infrastructure.Persistence.Configurations
{
    public sealed class ContextTicketMediaConfiguration : IEntityTypeConfiguration<TicketMediaModel>
    {
        public void Configure(EntityTypeBuilder<TicketMediaModel> entity)
        {
            entity.ToTable("ticket_media");

            entity.HasKey(ticketMedia => ticketMedia.TicketMediaId);

            entity.Property(ticketMedia => ticketMedia.TicketMediaId)
                .HasColumnName("ticket_media_id")
                .ValueGeneratedOnAdd();

            entity.Property(ticketMedia => ticketMedia.TicketMediaTicketId)
                .HasColumnName("ticket_media_ticket_id")
                .IsRequired();

            entity.Property(ticketMedia => ticketMedia.TicketMediaImageUrl)
                .HasColumnName("ticket_media_image_url")
                .HasMaxLength(255)
                .IsRequired();

            entity.HasOne<TicketModel>()
                .WithMany(ticket => ticket.TicketMedias)
                .HasForeignKey(ticketMedia => ticketMedia.TicketMediaTicketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}