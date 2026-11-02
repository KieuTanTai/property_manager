using System.ComponentModel.DataAnnotations;
using Shared.ModelHelper;

namespace TicketAndNotification.Models.Ticket
{
    public class TicketMediaModel
    {
        public TicketMediaModel(Guid ticketId, string imageUrl)
        {
            TicketMediaTicketId = ticketId;
            TicketMediaImageUrl = ImageDataUriGuard.Required(imageUrl, 300 * 1024, nameof(imageUrl));
        }

        public TicketMediaModel(int ticketMediaId, Guid ticketId, string imageUrl)
        {
            TicketMediaId = ticketMediaId;
            TicketMediaTicketId = ticketId;
            TicketMediaImageUrl = ImageDataUriGuard.Required(imageUrl, 300 * 1024, nameof(imageUrl));
        }

        public TicketMediaModel() {}

        public int TicketMediaId { get; init; }

        public Guid TicketMediaTicketId { get; private set; }

        [MaxLength(ImageDataUriGuard.StandardMaxEncodedCharacters)]
        public string TicketMediaImageUrl { get; private set; } = string.Empty;

        #region Setter

        public void SetTicketMediaTicketId(Guid ticketId)
        {
            TicketMediaTicketId = ticketId;
        }

        public void SetTicketMediaImageUrl(string imageUrl)
        {
            TicketMediaImageUrl = ImageDataUriGuard.Required(imageUrl, 300 * 1024, nameof(imageUrl));
        }

        #endregion
    }
}
