using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace TicketAndNotification.Models.Ticket
{
    public class TicketModel
    {
        public TicketModel(Guid accountId, string content, ETicketType type = ETicketType.Feedback)
        {
            TicketAccountId = accountId;
            TicketContent = content;
            TicketType = type;
        }

        public TicketModel(
            Guid ticketId,
            Guid accountId,
            string content,
            ETicketType type,
            bool isResolved)
        {
            TicketId = ticketId;
            TicketAccountId = accountId;
            TicketContent = content;
            TicketType = type;
            TicketIsResolved = isResolved;
        }

        public TicketModel() {}

        public Guid TicketId { get; init; }

        public Guid TicketAccountId { get; private set; }

        [Required] [MaxLength(255)]
        public string TicketContent { get; private set; } = string.Empty;

        public ETicketType TicketType { get; private set; } = ETicketType.Feedback;

        public bool TicketIsResolved { get; private set; }

        public DateTime TicketCreatedAt { get; init; } = DateTime.Now;

        public DateTime TicketUpdatedAt { get; private set; } = DateTime.Now;

        public IReadOnlyList<TicketMediaModel> TicketMedias { get; private set; } =
            new List<TicketMediaModel>();

        #region Setter

        public void SetTicketAccountId(Guid accountId)
        {
            TicketAccountId = accountId;
        }

        public void SetTicketContent(string content)
        {
            TicketContent = content;
        }

        public void SetTicketType(ETicketType type)
        {
            TicketType = type;
        }

        public void SetTicketIsResolved(bool isResolved)
        {
            TicketIsResolved = isResolved;
        }

        public void SetTicketMedias(IReadOnlyList<TicketMediaModel> ticketMedias)
        {
            TicketMedias = ticketMedias;
        }

        #endregion
    }
}