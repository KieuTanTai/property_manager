using System.ComponentModel.DataAnnotations;

namespace TicketAndNotification.Models.Ticket
{
    public class TicketMediaModel
    {
        public TicketMediaModel(Guid ticketId, string imageUrl)
        {
            TicketMediaTicketId = ticketId;
            TicketMediaImageUrl = imageUrl;
        }

        public TicketMediaModel(int ticketMediaId, Guid ticketId, string imageUrl)
        {
            TicketMediaId = ticketMediaId;
            TicketMediaTicketId = ticketId;
            TicketMediaImageUrl = imageUrl;
        }

        public TicketMediaModel() {}

        public int TicketMediaId { get; init; }

        public Guid TicketMediaTicketId { get; private set; }

        [MaxLength(255)]
        public string TicketMediaImageUrl { get; private set; } = string.Empty;
        
        #region Setter
        
        public void SetTicketMediaTicketId(Guid ticketId)
        {
            TicketMediaTicketId = ticketId;
        }

        public void SetTicketMediaImageUrl(string imageUrl)
        {
            TicketMediaImageUrl = imageUrl;
        }
        
        #endregion
    }
}